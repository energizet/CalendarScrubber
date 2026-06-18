using System.Net;
using Android.Webkit;
using CalendarScrubber.Services;
using WebView = Microsoft.Maui.Controls.WebView;

namespace CalendarScrubber;

public class CookieExtractor : ICookieExtractor
{
	public Task<CookieContainer> GetCookiesAsync(WebView webView, string url)
	{
		var container = new CookieContainer();
		var cookieManager = CookieManager.Instance!;

		// Синхронизация
		cookieManager.Flush();

		// Получаем строку
		var cookieString = cookieManager.GetCookie(url);

		if (string.IsNullOrEmpty(cookieString))
			return Task.FromResult(container);

		container = ParseSetCookieHeader(url, cookieString.Split(';'));

		return Task.FromResult(container);
	}

	public CookieContainer ParseSetCookieHeader(string url, IEnumerable<string> setCookieHeaders)
	{
		var uri = new Uri(url);
		var container = new CookieContainer();

		foreach (var header in setCookieHeaders)
		{
			// Берем первую часть (имя=значение) до первого ';'
			var firstPart = header.Split(';')[0];
			var parts = firstPart.Trim().Split('=');
			if (parts.Length >= 2)
			{
				var key = parts[0].Trim();
				// Берем всё после первого равно как значение
				var val = firstPart.Trim().Substring(key.Length + 1);

				try
				{
					// Важно: Path = "/" и Domain = uri.Host
					container.Add(new Cookie(key, val, "/", uri.Host));
				}
				catch { /* игнорируем битые куки */ }
			}
		}

		return container;
	}
}