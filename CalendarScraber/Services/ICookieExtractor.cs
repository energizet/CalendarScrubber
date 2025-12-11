using System.Net;

namespace CalendarScraber.Services;

public interface ICookieExtractor
{
	Task<CookieContainer> GetCookiesAsync(WebView webView, string url);
}