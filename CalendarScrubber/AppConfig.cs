namespace CalendarScrubber;

public static class AppConfig
{
	// === Базовые домены ===
	public const string BaseDomain = "https://chatzone.o3t.ru";
	public const string AuthCookieName = "MMAUTHTOKEN";

	// === Настройки SSO ===
	// URL, куда возвращается пользователь после логина
	public const string LoginUrl = $"{BaseDomain}/oauth/openid/login";

	// === Настройки API ===
	public const string CalendarEndpoint = "/api/v5/calendar/event/my";
}