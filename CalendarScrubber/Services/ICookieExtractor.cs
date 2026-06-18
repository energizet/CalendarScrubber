using System.Net;

namespace CalendarScrubber.Services;

public interface ICookieExtractor
{
	Task<CookieContainer> GetCookiesAsync(WebView webView, string url);
	CookieContainer ParseSetCookieHeader(string url, IEnumerable<string> setCookieHeaders);
}