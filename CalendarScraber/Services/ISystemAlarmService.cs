using CalendarScraber.Models;

namespace CalendarScraber.Services;

public interface ISystemAlarmService
{
	// Оставляем ID (для логики системы) и JSON (как контейнер данных)
	void SetAlarm(int hour, int minute, CalendarView ev);
	void CancelAlarm(CalendarView ev);
}