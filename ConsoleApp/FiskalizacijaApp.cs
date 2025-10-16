using ConsoleApp.Helpers;

using Fiskalizacija1;

using Microsoft.Extensions.Options;

namespace ConsoleApp;

public class FiskalizacijaApp
{
	private readonly FiskalizacijaOptions options;

	public FiskalizacijaApp(IOptions<FiskalizacijaOptions> eRacunData)
	{
		options = eRacunData.Value;
	}

	public async Task Run()
	{
		var certificate = CertTools.LoadCertificateFile(options.CertificateFileName, options.CertificatePassword);

		var fiscalization = new Fiskalizacija1Service(certificate);

		////-----------------------------------
		//// PRIJAVI RADNO VRIJEME
		////-----------------------------------

		//var resultPrijaviRadnoVrijeme = await fiscalization.PrijaviRadnoVrijeme(options.Oib, options.Oib, "1");

		////-----------------------------------
		//// DOHVATI RADNO VRIJEME
		////-----------------------------------

		//var resultDohvatiRadnoVrijeme = await fiscalization.DohvatiRadnoVrijeme(options.Oib, "1");

		//-----------------------------------
		// ECHO
		//-----------------------------------

		var resultEcho = await fiscalization.Echo("hello");

		//-----------------------------------
		// FISKALIZACIJA
		//-----------------------------------

		var invoice = RacunGenerator.Kreiraj(options.Oib);

		var resultFiskaliziraj = await fiscalization.Fiskaliziraj(invoice);

		Console.WriteLine($"Jir: {resultFiskaliziraj.RacunOdgovor.Jir}");

		Console.WriteLine($"Zki: {invoice.ZastKod}");

		//-----------------------------------
		// PROMIJENI PODATKE RAČUNA
		//-----------------------------------
		var racunPPType = new RacunPPType
		{
			BrRac = invoice.BrRac,
			DatVrijeme = invoice.DatVrijeme,
			Oib = invoice.Oib,
			IznosMarza = invoice.IznosMarza,
			IznosNePodlOpor = invoice.IznosNePodlOpor,
			IznosOslobPdv = invoice.IznosOslobPdv,
			IznosUkupno = invoice.IznosUkupno,
			NacinPlac = NacinPlacanjaType.K,
			NakDost = invoice.NakDost,
			Naknade = invoice.Naknade,
			OibOper = invoice.OibOper,
			OibPrimateljaRacuna = invoice.OibPrimateljaRacuna,
			OznSlijed = invoice.OznSlijed,
			OstaliPor = invoice.OstaliPor,
			ParagonBrRac = invoice.ParagonBrRac,
			Pdv = invoice.Pdv,
			Pnp = invoice.Pnp,
			PromijenjeniNacinPlac = NacinPlacanjaType.T,
			USustPdv = invoice.USustPdv,
			ZastKod = invoice.ZastKod,
			SpecNamj = invoice.SpecNamj,
		};
		var resultPromjeniPodatkeRacuna = await fiscalization.PromijeniPodatkeRacuna(racunPPType, resultFiskaliziraj.RacunOdgovor.Zaglavlje.IdPoruke);

		//if (resultPromjeniPodatkeRacuna.PromijeniPodatkeRacunaOdgovor.PorukaOdgovora.SifraPoruke != "p001")
		//{
		//	throw new Exception("Error changing invoice: " + resultPromjeniPodatkeRacuna.PromijeniPodatkeRacunaOdgovor.PorukaOdgovora.Poruka);
		//}

		//-----------------------------------
		// PROMJENA NAĆINA PLAĆANJA
		//-----------------------------------
		var racunPNP = new RacunPNPType
		{
			BrRac = invoice.BrRac,
			DatVrijeme = invoice.DatVrijeme,
			Oib = invoice.Oib,
			IznosMarza = invoice.IznosMarza,
			IznosNePodlOpor = invoice.IznosNePodlOpor,
			IznosOslobPdv = invoice.IznosOslobPdv,
			IznosUkupno = invoice.IznosUkupno,
			NacinPlac = NacinPlacanjaType.K,
			NakDost = invoice.NakDost,
			Naknade = invoice.Naknade,
			OibOper = invoice.OibOper,
			OibPrimateljaRacuna = invoice.OibPrimateljaRacuna,
			OznSlijed = invoice.OznSlijed,
			OstaliPor = invoice.OstaliPor,
			ParagonBrRac = invoice.ParagonBrRac,
			Pdv = invoice.Pdv,
			Pnp = invoice.Pnp,
			PromijenjeniNacinPlac = NacinPlacanjaType.K,
			USustPdv = invoice.USustPdv,
			ZastKod = invoice.ZastKod,
			SpecNamj = invoice.SpecNamj,
		};
		var resultPromjeniNacinPlacanja = await fiscalization.PromijeniNacPlac(racunPNP, resultFiskaliziraj.RacunOdgovor.Zaglavlje.IdPoruke);

		if (resultPromjeniNacinPlacanja.PromijeniNacPlacOdgovor.PorukaOdgovora.SifraPoruke != "p001")
		{
			throw new Exception("Error changing payment method: " + resultPromjeniNacinPlacanja.PromijeniNacPlacOdgovor.PorukaOdgovora.Poruka);
		}

		//-----------------------------------
		// PROVJERA RAČUNA
		//-----------------------------------
		var resultProvjeri = await fiscalization.Provjeri(invoice);

		Console.WriteLine($"Zki: {resultProvjeri.ProvjeraOdgovor.Racun.ZastKod}");
	}
}
