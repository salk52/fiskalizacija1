using System;

namespace Fiskalizacija1.Extensions;

public static class DateExtensions
{
	public static string ToShortString(this DateTime dateTime)
	{
		return $"{dateTime:dd.MM.yyyy}";
	}

	public static string ToLongString(this DateTime dateTime)
	{
		return $"{dateTime:dd.MM.yyyyTHH:mm:ss}";
	}
}
