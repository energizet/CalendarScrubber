using CalendarScraber.Models;

namespace CalendarScraber.Services;

public interface ISystemAlarmService
{
	// Оставляем ID (для логики системы) и JSON (как контейнер данных)
	void SetAlarm(DateTime alarmTime, CalendarView ev);
	void CancelAlarm(CalendarView ev);
	void CancelNotification(string eventId);
}