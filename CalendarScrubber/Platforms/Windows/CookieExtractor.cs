using System.Net;
using CalendarScrubber.Services;
using Microsoft.UI.Xaml.Controls;

namespace CalendarScrubber;

public class CookieExtractor : ICookieExtractor
{
	public async Task<CookieContainer> GetCookiesAsync(Microsoft.Maui.Controls.WebView webView, string url)
	{
		var container = new CookieContainer();

		// Достаем нативный WebView2
		if (webView.Handler?.PlatformView is WebView2 w2)
		{
			try
			{
				var cookieManager = w2.CoreWebView2.CookieManager;
				var cookies = await cookieManager.GetCookiesAsync(url);

				if (cookies != null)
				{
					foreach (var c in cookies)
					{
						// Конвертируем WebView2 Cookie в System.Net.Cookie
						container.Add(new Cookie(c.Name, c.Value, c.Path, c.Domain));
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Windows Cookie Error: {ex.Message}");
			}
		}

		return container;
	}
}