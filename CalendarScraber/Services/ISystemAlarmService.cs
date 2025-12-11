namespace CalendarScraber.Services;

public interface ISystemAlarmService
{
	// hour: 0-23, minute: 0-59
	void SetAlarm(int hour, int minute, string title);
}