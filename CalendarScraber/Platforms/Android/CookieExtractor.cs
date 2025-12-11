using System.Net;
using Android.Webkit;
using CalendarScraber.Services;
using WebView = Microsoft.Maui.Controls.WebView;

namespace CalendarScraber;

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

		// Парсинг строки (Android specific logic)
		// Привязываем куки к хосту, чтобы HttpClient их подхватил
		var uri = new Uri(url);
		// Обычно для API нужен BaseDomain, но тут берем хост из URL
		// (или можно внедрить AppConfig сюда же, но лучше передать url параметром)
        
		var pairs = cookieString.Split(';');
		foreach (var pair in pairs)
		{
			var parts = pair.Trim().Split('=');
			if (parts.Length >= 2)
			{
				var key = parts[0].Trim();
				// Берем всё после первого равно как значение
				var val = pair.Trim().Substring(key.Length + 1);

				try 
				{
					// Важно: Path = "/" и Domain = uri.Host
					container.Add(new Cookie(key, val, "/", uri.Host));
				}
				catch { /* игнорируем битые куки */ }
			}
		}

		return Task.FromResult(container);
	}
}