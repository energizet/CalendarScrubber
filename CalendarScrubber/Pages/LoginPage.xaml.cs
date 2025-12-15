using System.Net;
using CalendarScrubber.Services;

namespace CalendarScrubber.Pages;

public partial class LoginPage : ContentPage
{
	private readonly ICookieExtractor _cookieExtractor;
	public event Action<CookieContainer>? OnLoginSuccess;
	private bool _isSuccessDetected = false;

	public LoginPage(ICookieExtractor cookieExtractor)
	{
		InitializeComponent();
		_cookieExtractor = cookieExtractor;
		AuthWebView.Source = AppConfig.LoginUrl;
	}

	// Метод для перехвата редиректа
	private async void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
	{
		if (_isSuccessDetected) return;
		
		AppLogger.Log($"WebView: Загружено {e.Url}. Ищем куки...");

		// ВМЕСТО проверки URL, мы проверяем наличие "Золотой Куки"
		// Мы просим экстрактор дать куки для Базового Домена (куда эта кука должна быть установлена)
		var container = await _cookieExtractor.GetCookiesAsync(AuthWebView, AppConfig.BaseDomain);

		// Ищем нужную куку в контейнере
		var authCookie = GetCookieFromContainer(container, AppConfig.BaseDomain, AppConfig.AuthCookieName);

		if (authCookie != null)
		{
			_isSuccessDetected = true;
			AppLogger.Log($"🎉 КУКА НАЙДЕНА: {authCookie.Name}");
            
			await CookieStorage.SaveCookies(container, AppConfig.BaseDomain);
			System.Diagnostics.Debug.WriteLine($"✅ Авторизация успешна! Кука {authCookie.Name} найдена.");
            
			// Прячем WebView, чтобы пользователь не нажимал лишнего
			AuthWebView.IsVisible = false;
			LoadingSpinner.IsRunning = true;

			OnLoginSuccess?.Invoke(container);
			await Navigation.PopModalAsync();
		}
	}

	// Вспомогательный метод для поиска куки в контейнере
	private Cookie? GetCookieFromContainer(CookieContainer container, string domainUrl, string cookieName)
	{
		try
		{
			var uri = new Uri(domainUrl);
			var cookies = container.GetCookies(uri);

			// Ищем куку по имени (игнорируя регистр, если нужно)
			foreach (Cookie cookie in cookies)
			{
				if (cookie.Name.Equals(cookieName, StringComparison.OrdinalIgnoreCase))
				{
					return cookie;
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Ошибка поиска куки: {ex.Message}");
		}

		return null;
	}

	private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
	{
	}
}