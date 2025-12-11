using System.Net;
using System.Text.Json;
using CalendarScraber.Models;

namespace CalendarScraber.Services;

public class CalendarService
{
	private HttpClient? _httpClient;

	// Инициализируем клиент, когда получили куки
	public void InitializeClient(CookieContainer cookies)
	{
		var handler = new HttpClientHandler
		{
			CookieContainer = cookies,
			UseCookies = true
		};

		_httpClient = new HttpClient(handler)
		{
			BaseAddress = new Uri(AppConfig.BaseDomain)
		};
	}

	public async Task<List<CalendarView>?> CheckEventsAsync()
	{
		if (_httpClient == null) return null;

		try
		{
			// Формируем даты
			var today = DateTime.Now.ToString("yyyy-MM-dd");
			var start = WebUtility.UrlEncode($"{today}T00:00:00+03:00");
			var end = WebUtility.UrlEncode($"{today}T23:59:59+03:00");

			var url = $"{AppConfig.CalendarEndpoint}?start={start}&end={end}";

			var response = await _httpClient.GetAsync(url);
            
			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<CalendarResponse>(json, new System.Text.Json.JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});
				return result?.Views;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка запроса: {ex.Message}");
		}
		return null;
	}
}