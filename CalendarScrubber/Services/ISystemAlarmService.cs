using CalendarScrubber.Models;

namespace CalendarScrubber.Services;

public interface ISystemAlarmService
{
	// Оставляем ID (для логики системы) и JSON (как контейнер данных)
	void SetAlarm(DateTime alarmTime, CalendarView ev);
	void CancelAlarm(CalendarView ev);
	void CancelNotification(string eventId);
}