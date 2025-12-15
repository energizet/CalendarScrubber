using System.Net;

namespace CalendarScrubber.Services;

public interface ICookieExtractor
{
	Task<CookieContainer> GetCookiesAsync(WebView webView, string url);
}