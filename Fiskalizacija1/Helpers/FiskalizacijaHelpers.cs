using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

using System.Xml.Serialization;

using Fiskalizacija1.Interfaces;

namespace Fiskalizacija1;

public class FiskalizacijaHelpers
{
	public static void Sign(IZahtjev request, X509Certificate2 certificate)
	{
		if (request.Signature is not null) return;

		if (request is not null && request is RacunZahtjev rz && rz.Racun.ZastKod == null)
		{
			GenerateZki(rz.Racun, certificate);
		}

		request.Id = request.GetType().Name;

		var ser = Serialize(request);
		var doc = new XmlDocument();
		doc.LoadXml(ser);

		var xml = new SignedXml(doc);

		xml.SigningKey = certificate.GetRSAPrivateKey();
		xml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

		var keyInfo = new KeyInfo();
		var keyInfoData = new KeyInfoX509Data();

		keyInfoData.AddCertificate(certificate);
		keyInfoData.AddIssuerSerial(certificate.Issuer, certificate.GetSerialNumberString());
		keyInfo.AddClause(keyInfoData);

		xml.KeyInfo = keyInfo;

		var transforms = new Transform[]
		{
			new XmlDsigEnvelopedSignatureTransform(false),
			new XmlDsigExcC14NTransform(false)
		};

		var reference = new Reference($"#{request.Id}");

		foreach (var x in transforms)
		{
			reference.AddTransform(x);
		}

		xml.AddReference(reference);
		xml.ComputeSignature();

		var s = xml.Signature;
		var certSerial = (X509IssuerSerial)keyInfoData.IssuerSerials[0];

		request.Signature = new SignatureType
		{
			SignedInfo = new SignedInfoType
			{
				CanonicalizationMethod = new CanonicalizationMethodType { Algorithm = s.SignedInfo.CanonicalizationMethod },
				SignatureMethod = new SignatureMethodType { Algorithm = s.SignedInfo.SignatureMethod },
				Reference =
					(from x in s.SignedInfo.References.OfType<Reference>()
					 select new ReferenceType
					 {
						 URI = x.Uri,
						 Transforms =
							 (from t in transforms
							  select new TransformType { Algorithm = t.Algorithm }).ToArray(),
						 DigestMethod = new DigestMethodType { Algorithm = x.DigestMethod },
						 DigestValue = x.DigestValue
					 }).ToArray()
			},
			SignatureValue = new SignatureValueType { Value = s.SignatureValue },
			KeyInfo = new KeyInfoType
			{
				ItemsElementName = new[] { ItemsChoiceType2.X509Data },
				Items = new[]
				{
					new X509DataType
					{
						ItemsElementName = new[]
						{
							ItemsChoiceType.X509IssuerSerial,
							ItemsChoiceType.X509Certificate
						},
						Items = new object[]
						{
							new X509IssuerSerialType
							{
								X509IssuerName = certSerial.IssuerName,
								X509SerialNumber = certSerial.SerialNumber
							},
							certificate.RawData
						}
					}
				}
			}
		};
	}

	public static void GenerateZki(RacunType invoice, X509Certificate2 certificate)
	{
		if (certificate == null) throw new ArgumentNullException("certificate");

		var sb = new StringBuilder();

		sb.Append(invoice.Oib);
		sb.Append(invoice.DatVrijeme);
		sb.Append(invoice.BrRac.BrOznRac);
		sb.Append(invoice.BrRac.OznPosPr);
		sb.Append(invoice.BrRac.OznNapUr);
		sb.Append(invoice.IznosUkupno);

		invoice.ZastKod = SignAndHashMD5(sb.ToString(), certificate);
	}

	public static string SignAndHashMD5(string value, X509Certificate2 certificate)
	{
		if (value == null) throw new ArgumentNullException("value");
		if (certificate == null) throw new ArgumentNullException("certificate");

		var b = Encoding.ASCII.GetBytes(value);

		var provider = certificate.GetRSAPrivateKey()!;

		var signData = provider.SignData(b, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

		var md5 = MD5.Create();
		var hash = md5.ComputeHash(signData);
		var result = Convert.ToHexString(hash).ToLower();

		return result;
	}

	public static bool CheckSignature(RacunZahtjev request)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentNullException.ThrowIfNull(request.Signature);

		var doc = new XmlDocument();
		var ser = Serialize(request);
		doc.LoadXml(ser);

		return CheckSignatureXml(doc);
	}

	public static bool CheckSignatureXml(XmlDocument doc)
	{
		ArgumentNullException.ThrowIfNull(doc);

		var signatureNode = doc.GetElementsByTagName("Signature").Item(0) as XmlElement;
		var signedXml = new SignedXml((XmlElement)doc.DocumentElement.FirstChild.FirstChild);
		signedXml.LoadXml(signatureNode);

		return signedXml.CheckSignature();
	}

	private static string Serialize(IZahtjev request)
	{
		using var ms = new MemoryStream();

		var root = new XmlRootAttribute
		{
			Namespace = "http://www.apis-it.hr/fin/2012/types/f73",
			IsNullable = false
		};

		var serializer = new XmlSerializer(request.GetType(), root);
		serializer.Serialize(ms, request);

		return Encoding.UTF8.GetString(ms.ToArray());
	}

	public static void ThrowOnResponseErrors(IGreska response)
	{
		var greske = response?.Greske ?? new GreskaType[] { };

		if (response is ProvjeraOdgovor)
		{
			// Ako ima v100 onda je ok, piše u dokumentaciji
			// Remove "valid error" from response
			greske = greske.Where(x => x.SifraGreske != "v100").ToArray();
			response.Greske = greske;
		}

		if (greske.Any())
		{
			var errorList = greske.Select(x => $"{x.SifraGreske} - {x.PorukaGreske}");

			var errorMessage = string.Join(Environment.NewLine, errorList);

			throw new Exception($"Greške: {errorMessage}");
		}
	}
}
