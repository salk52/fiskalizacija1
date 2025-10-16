using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;

using Fiskalizacija1.Extensions;

namespace Fiskalizacija1;

public partial class Fiskalizacija1Service
{
	public static string Url_Demo { get; } = "https://cistest.apis-it.hr:8449/FiskalizacijaServiceTest";
	public static string Url_Prod { get; } = "https://cis.porezna-uprava.hr:8449/FiskalizacijaService";

	public Fiskalizacija1Service(X509Certificate2 certificate, bool isDemo = true)
	{
		Certificate = certificate;
		IsDemo = isDemo;
	}

	public X509Certificate2 Certificate { get; }
	public bool IsDemo { get; }

	FiskalizacijaPortTypeClient webClient;

	private FiskalizacijaPortTypeClient GetClient()
	{
		if (webClient is not null)
		{
			return webClient;
		}

		webClient = new FiskalizacijaPortTypeClient();

		webClient.Endpoint.Address = IsDemo ? new EndpointAddress(Url_Demo) : new EndpointAddress(Url_Prod);

		if (IsDemo)
		{
			webClient.ChannelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
			{
				CertificateValidationMode = X509CertificateValidationMode.None,
				RevocationMode = X509RevocationMode.NoCheck
			};
		}

		return webClient;
	}

	public async Task<echoResponse> Echo(string echo)
	{
		var webClient = GetClient();

		return await webClient.echoAsync(echo);
	}

	public async Task<racuniResponse> Fiskaliziraj(RacunType invoice)
	{
		var racunZahtjev = new RacunZahtjev
		{
			Racun = invoice,
			Zaglavlje = GetRequestHeader()
		};

		FiskalizacijaHelpers.Sign(racunZahtjev, Certificate);

		var webService = GetClient();

		var response = await webService.racuniAsync(racunZahtjev);

		FiskalizacijaHelpers.ThrowOnResponseErrors(response.RacunOdgovor);

		return response;
	}

	public async Task<provjeraResponse> Provjeri(RacunType racun)
	{
		var provjeraZahtjev = new ProvjeraZahtjev
		{
			Racun = racun,
			Zaglavlje = GetRequestHeader()
		};

		FiskalizacijaHelpers.Sign(provjeraZahtjev, Certificate);

		var webClient = GetClient();

		var response = await webClient.provjeraAsync(provjeraZahtjev);

		FiskalizacijaHelpers.ThrowOnResponseErrors(response.ProvjeraOdgovor);

		return response;
	}

	public async Task<promijeniPodatkeRacunaResponse> PromijeniPodatkeRacuna(RacunPPType racun, string IdPoruke)
	{
		var promijeniPodatkeRacunaZahtjev = new PromijeniPodatkeRacunaZahtjev
		{
			Racun = racun,
			Zaglavlje = GetRequestHeader()
		};

		promijeniPodatkeRacunaZahtjev.Zaglavlje.IdPoruke = IdPoruke;

		FiskalizacijaHelpers.Sign(promijeniPodatkeRacunaZahtjev, Certificate);

		var webService = GetClient();

		var response = await webService.promijeniPodatkeRacunaAsync(promijeniPodatkeRacunaZahtjev);

		FiskalizacijaHelpers.ThrowOnResponseErrors(response.PromijeniPodatkeRacunaOdgovor);

		return response;
	}

	public async Task<promijeniNacPlacResponse> PromijeniNacPlac(RacunPNPType racun, string IdPoruke)
	{
		var promijeniNacPlacZahtjev = new PromijeniNacPlacZahtjev
		{
			Racun = racun,
			Zaglavlje = GetRequestHeader()
		};

		promijeniNacPlacZahtjev.Zaglavlje.IdPoruke = IdPoruke;

		FiskalizacijaHelpers.Sign(promijeniNacPlacZahtjev, Certificate);

		var webClient = GetClient();

		var response = await webClient.promijeniNacPlacAsync(promijeniNacPlacZahtjev);

		FiskalizacijaHelpers.ThrowOnResponseErrors(response.PromijeniNacPlacOdgovor);

		return response;
	}

	public async Task<dohvatiRadnoVrijemeResponse> DohvatiRadnoVrijeme(string Oib, string OznPosPr)
	{
		var dohvatiRadnoVrijemeZahtjev = new DohvatiRadnoVrijemeZahtjev
		{
			Oib = Oib,
			OznPosPr = OznPosPr,
			VrstaRadnogVremena = DohvatiRadnoVrijemeZahtjevVrstaRadnogVremena.SVE,
			Zaglavlje = GetRequestHeader()
		};

		FiskalizacijaHelpers.Sign(dohvatiRadnoVrijemeZahtjev, Certificate);

		var webService = GetClient();

		var response = await webService.dohvatiRadnoVrijemeAsync(dohvatiRadnoVrijemeZahtjev);

		FiskalizacijaHelpers.ThrowOnResponseErrors(response.DohvatiRadnoVrijemeOdgovor);

		return response;
	}

	public async Task<prijaviRadnoVrijemeResponse> PrijaviRadnoVrijeme(string Oib, string OibOper, string OznPosPr)
	{
		var prijaviRadnoVrijemeZahtjev = new PrijaviRadnoVrijemeZahtjev
		{
			PoslovniProstor = new PoslovniProstorType
			{
				Item = new RadnoVrijemeType
				{
					Redovno = new RedovnoType[]
					 {
						  new RedovnoType {
							  DatumOd =  DateTime.Now.ToShortString(),
							  DatumDo =  DateTime.Now.AddDays(30).ToShortString(),
							  Napomena = "Radno vrijeme",
							  Items = [new JednokratnoType {
								  DanUTjednu= DanUTjednuType.Item1,
								  RadnoVrijemeOd = DateTime.Now.ToShortString(),
								  RadnoVrijemeDo = DateTime.Now.AddDays(30).ToShortString()
							  }]
						  },
					 },
				},
				Oib = Oib,
				OibOper = Oib,
				OznPosPr = OznPosPr
			},
			Zaglavlje = GetRequestHeader()
		};

		FiskalizacijaHelpers.Sign(prijaviRadnoVrijemeZahtjev, Certificate);

		var webService = GetClient();

		var response = await webService.prijaviRadnoVrijemeAsync(prijaviRadnoVrijemeZahtjev);

		FiskalizacijaHelpers.ThrowOnResponseErrors(response.PrijaviRadnoVrijemeOdgovor);

		return response;
	}

	public static ZaglavljeType GetRequestHeader()
	{
		return new ZaglavljeType
		{
			IdPoruke = Guid.NewGuid().ToString(),
			DatumVrijeme = DateTime.Now.ToLongString()
		};
	}
}
