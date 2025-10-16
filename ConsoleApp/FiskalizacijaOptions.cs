using System.Security.Cryptography.X509Certificates;

namespace ConsoleApp;

public class FiskalizacijaOptions
{
	public const string Key = "Fiskalizacija";

	public string Oib { get; set; }
	public bool IsDemo { get; set; }
	public string CertificateFileName { get; set; }
	public string CertificatePassword { get; set; }
}
