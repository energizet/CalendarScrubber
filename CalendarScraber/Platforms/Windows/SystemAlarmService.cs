using CalendarScraber.Services;

namespace CalendarScraber;

public class SystemAlarmService : ISystemAlarmService
{
	public void SetAlarm(int hour, int minute, string title)
	{
		// Windows не имеет простого API для установки будильника
		System.Diagnostics.Debug.WriteLine("Установка системного будильника не поддерживается на Windows");
	}
}