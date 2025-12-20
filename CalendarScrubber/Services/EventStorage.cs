using System.Text.Json;
using CalendarScrubber.Models;

namespace CalendarScrubber.Services;

public class EventStorage : IEventStorage
{
    private const string StorageKey = "cached_calendar_events";
    
    // Кэш в памяти, чтобы не читать диск каждый раз
    private List<CalendarView>? _memoryCache;
    
    // Лок для потокобезопасности (на случай если UI и Сервис полезут одновременно)
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task SaveEventsAsync(List<CalendarView>? events)
    {
        await _semaphore.WaitAsync();
        try
        {
            // 1. Обновляем память
            _memoryCache = events ?? [];

            // 2. Сериализуем в JSON
            var json = JsonSerializer.Serialize(_memoryCache);

            // 3. Сохраняем в Preferences
            Preferences.Set(StorageKey, json);
            
            AppLogger.Log($"💾 Storage: Сохранено {_memoryCache.Count} событий.");
        }
        catch (Exception ex)
        {
            AppLogger.Log($"❌ Storage Error (Save): {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<List<CalendarView>> GetAllEventsAsync()
    {
        // Если уже есть в памяти - возвращаем сразу
        if (_memoryCache != null)
        {
            return _memoryCache;
        }

        await _semaphore.WaitAsync();
        try
        {
            // Проверяем еще раз (double-check locking)
            if (_memoryCache != null) return _memoryCache;

            // Читаем из Preferences
            var json = Preferences.Get(StorageKey, string.Empty);

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    _memoryCache = JsonSerializer.Deserialize<List<CalendarView>>(json);
                }
                catch
                {
                    // ignored
                }
            }

            return _memoryCache ??= [];
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<CalendarView?> GetEventAsync(string eventId)
    {
        // Убеждаемся, что данные загружены
        var allEvents = await GetAllEventsAsync();

        // Ищем событие
        return allEvents.FirstOrDefault(e => e.ItemId.Id == eventId);
    }

    public void ClearEvents()
    {
        // Тут lock не так критичен, но можно добавить для порядка
        _memoryCache = null;
        Preferences.Remove(StorageKey);
        AppLogger.Log("🗑️ Storage: Все события удалены.");
    }
}