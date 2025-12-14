using CalendarScraber.Models;
using CalendarScraber.Services;

namespace CalendarScraber;

public class SystemAlarmService : ISystemAlarmService
{
    // Метод установки будильника
    public void SetAlarm(int hour, int minute, CalendarView ev)
    {
        System.Diagnostics.Debug.WriteLine($"[Windows Stub] Alarm set for {hour}:{minute} (ID: {ev.ItemId.Id})");
    }

    // Метод отмены будильника
    public void CancelAlarm(CalendarView ev)
    {
        System.Diagnostics.Debug.WriteLine($"[Windows Stub] Alarm cancelled (ID: {ev.ItemId.Id})");
    }
}