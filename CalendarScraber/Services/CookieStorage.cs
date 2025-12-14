using System.Net;
using System.Text.Json;

namespace CalendarScraber.Services;

public static class CookieStorage
{
	private const string CookiesKey = "saved_auth_cookies";

	// Простая модель для сохранения данных куки
	public class CookieDto
	{
		public string Name { get; set; } = string.Empty;
		public string Value { get; set; } = string.Empty;
		public string Domain { get; set; } = string.Empty;
		public string Path { get; set; } = string.Empty;
	}

	// === СОХРАНЕНИЕ ===
	public static async Task SaveCookies(CookieContainer container, string url)
	{
		try
		{
			var uri = new Uri(url);
			var cookies = container.GetCookies(uri);
			var list = new List<CookieDto>();

			foreach (Cookie c in cookies)
			{
				list.Add(new CookieDto
				{
					Name = c.Name,
					Value = c.Value,
					Domain = c.Domain,
					Path = c.Path
				});
			}

			if (list.Count > 0)
			{
				var json = JsonSerializer.Serialize(list);
				// Используем Preferences (или SecureStorage.SetAsync для большей безопасности)
				await SecureStorage.SetAsync(CookiesKey, json);
				System.Diagnostics.Debug.WriteLine($"✅ Cookies saved: {list.Count}");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error saving cookies: {ex.Message}");
		}
	}

	// === ЗАГРУЗКА ===
	public static async Task<CookieContainer> LoadCookies()
	{
		var container = new CookieContainer();
		var json = await SecureStorage.GetAsync(CookiesKey) ?? string.Empty;

		if (string.IsNullOrEmpty(json)) return container;

		try
		{
			var list = JsonSerializer.Deserialize<List<CookieDto>>(json);
			if (list != null)
			{
				foreach (var item in list)
				{
					// Восстанавливаем куку
					// Важно: Domain не должен начинаться с точки для HttpClient в некоторых случаях,
					// но для безопасности лучше брать хост из конфига, если кука была привязана к нему.
					container.Add(new Cookie(item.Name, item.Value, item.Path, item.Domain));
				}

				System.Diagnostics.Debug.WriteLine($"♻️ Cookies loaded: {list.Count}");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error loading cookies: {ex.Message}");
			// Если данные повреждены, лучше их очистить
			ClearCookies();
		}

		return container;
	}

	// === ОЧИСТКА (ВЫХОД) ===
	public static void ClearCookies()
	{
		Preferences.Remove(CookiesKey);
	}
}