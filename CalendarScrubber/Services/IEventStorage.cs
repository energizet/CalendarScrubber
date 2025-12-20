using CalendarScrubber.Models;

namespace CalendarScrubber.Services;

public interface IEventStorage
{
	// Сохраняет (перезаписывает) список событий
	void SaveEvents(List<CalendarView> events);

	// Возвращает все события (из памяти или загружает с диска)
	List<CalendarView> GetAllEvents();

	// Возвращает одно событие по ID
	CalendarView? GetEvent(string eventId);

	// Удаляет все события
	void ClearEvents();
}