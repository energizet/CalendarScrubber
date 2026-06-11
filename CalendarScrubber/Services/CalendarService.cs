using System.Net;
using System.Text.Json;
using CalendarScrubber.Models;
using CalendarView = CalendarScrubber.Models.CalendarView;

namespace CalendarScrubber.Services;

public class CalendarService
{
	private HttpClient _httpClient;

// Опции для десериализации (создаем один раз для оптимизации)
	private readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true // Игнорировать регистр (itemId == ItemId)
	};

	public CalendarService()
	{
		// Инициализируем клиент сразу, даже без кук
		// BaseDomain берем из конфига
		_httpClient = new()
		{
			BaseAddress = new(AppConfig.BaseDomain)
		};

		// Притворяемся браузером сразу
		_httpClient.DefaultRequestHeaders.Add("User-Agent",
			"Mozilla/5.0 (Android 13; Mobile; rv:109.0) Gecko/109.0 Firefox/112.0");
	}

	public void UpdateCookies(CookieContainer cookies)
	{
		var handler = new HttpClientHandler
		{
			CookieContainer = cookies,
			UseCookies = true,
			AllowAutoRedirect = false // Для API редиректы часто вредны
		};

		// Пересоздаем клиент с новыми куками
		_httpClient = new(handler)
		{
			BaseAddress = new(AppConfig.BaseDomain)
		};
	}

	public async Task<List<CalendarView>?> GetEventsAsync()
	{
		try
		{
			// 1. ЛОГИКА ДАТ: Следующие 24 часа в UTC
			var nowUtc = DateTime.UtcNow;
			var endUtc = nowUtc.AddHours(24);

			// 2. ФОРМАТИРОВАНИЕ: ISO 8601 с суффиксом Z
			// Пример: 2023-10-05T14:30:00Z
			var format = "yyyy-MM-ddTHH:mm:ssZ";

			var startParam = WebUtility.UrlEncode(nowUtc.ToString(format));
			var endParam = WebUtility.UrlEncode(endUtc.ToString(format));

			var url = $"{AppConfig.CalendarEndpoint}?start={startParam}&end={endParam}";

			HttpResponseMessage response;
			while (true)
			{
				AppLogger.Log($"🌍 Запрос данных: {url}");

				response = await _httpClient.GetAsync(url);

				AppLogger.Log($"📡 Ответ сервера: {response.StatusCode}");

				if (
					response.StatusCode is
					HttpStatusCode.MovedPermanently or
					HttpStatusCode.Redirect or
					HttpStatusCode.TemporaryRedirect or
					HttpStatusCode.PermanentRedirect
				)
				{
					var redirectUrl = response.Headers.Location;
					AppLogger.Log($"🔀 Обнаружен редирект. Location: {redirectUrl}");

					url = redirectUrl?.ToString();
					continue;
				}

				break;
			}

			// ГЛАВНОЕ ИЗМЕНЕНИЕ: Проверка статуса
			if (
				response.StatusCode is
				HttpStatusCode.Unauthorized or
				HttpStatusCode.Forbidden
			)
			{
				AppLogger.Log("❌ Ошибка авторизации (401/403)");
				// Выбрасываем специальное исключение, которое поймает MainPage
				throw new UnauthorizedAccessException("Требуется авторизация");
			}

			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();
				AppLogger.Log($"📦 Получено {json.Length} байт. Десериализация...");
				// 4. ДЕСЕРИАЛИЗАЦИЯ: Регистронезависимая
				var result = JsonSerializer.Deserialize<CalendarResponse>(json, _jsonOptions);
				AppLogger.Log($"✅ Найдено событий: {result?.Views.Count ?? 0}");
				return result?.Views ?? [];
			}

			AppLogger.Log($"⚠️ Ошибка сервера: {response.StatusCode}");
		}
		catch (HttpRequestException ex)
		{
			AppLogger.Log($"💀 Ошибка сети: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"Network error: {ex.Message}");
		}
		catch (JsonException ex) // Ловим ошибки парсинга JSON
		{
			AppLogger.Log($"💩 Ошибка JSON: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"JSON parse error: {ex.Message}");
		}

		return null;
	}
}