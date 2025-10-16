using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleApp.Helpers;

public class CertTools
{
	public static X509Certificate2 LoadCertificateFile(string CertificateFileName, string CertificatePassword)
	{
		var rootPath = AppContext.BaseDirectory;

		var fileName = Path.Combine(rootPath, "cert", CertificateFileName);

		if (!File.Exists(fileName))
		{
			throw new FileNotFoundException($"Certificate file not found: {fileName}");
		}

		return new X509Certificate2(fileName, CertificatePassword,
			X509KeyStorageFlags.MachineKeySet |
			X509KeyStorageFlags.PersistKeySet |
			X509KeyStorageFlags.Exportable);
	}
}
