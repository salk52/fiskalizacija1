using System.Globalization;

using Fiskalizacija1;
using Fiskalizacija1.Extensions;

namespace ConsoleApp;

public class RacunGenerator
{
	public static RacunType Kreiraj(string Oib)
	{
		return new RacunType()
		{
			BrRac = new BrojRacunaType()
			{
				BrOznRac = "1",
				OznPosPr = "1",
				OznNapUr = "1"
			},
			DatVrijeme = DateTime.Now.ToLongString(),
			IznosUkupno = 125.ToString("N2", CultureInfo.InvariantCulture),
			NacinPlac = NacinPlacanjaType.G,
			NakDost = false,
			Oib = Oib,
			OibOper = "12345678901",
			//OibPrimateljaRacuna = "85821130368",
			OznSlijed = OznakaSlijednostiType.P,
			Pdv = new[]
			{
				new PorezType
				{
					Stopa = 25.ToString("N2", CultureInfo.InvariantCulture),
					Osnovica = 100.ToString("N2", CultureInfo.InvariantCulture),
					Iznos = 25.ToString("N2", CultureInfo.InvariantCulture),
				}
			},
			USustPdv = true
		};
	}
}
