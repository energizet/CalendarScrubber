using CalendarScrubber.Models;
using CalendarScrubber.Services;

namespace CalendarScrubber;

public class SystemAlarmService : ISystemAlarmService
{
    // Метод установки будильника
    public void SetAlarm(DateTime alarmTime, CalendarView ev)
    {
        System.Diagnostics.Debug.WriteLine($"[Windows Stub] Alarm set for {alarmTime} (ID: {ev.ItemId.Id})");
    }

    // Метод отмены будильника
    public void CancelAlarm(string eventId)
    {
        System.Diagnostics.Debug.WriteLine($"[Windows Stub] Alarm cancelled (ID: {eventId})");
    }
    
    public void CancelNotification(string eventId)
    {
        // Windows stub: ничего не делаем
    }
}