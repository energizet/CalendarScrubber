using CalendarScrubber.Models;

namespace CalendarScrubber.Services;

public interface IEventStorage
{
	// Сохраняет (перезаписывает) список событий
	Task SaveEventsAsync(List<CalendarView> events);

	// Возвращает все события (из памяти или загружает с диска)
	Task<List<CalendarView>> GetAllEventsAsync();

	// Возвращает одно событие по ID
	Task<CalendarView?> GetEventAsync(string eventId);

	// Удаляет все события
	void ClearEvents();
}