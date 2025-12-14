using System.Text.Json;
using Android.App;
using Android.Content;
using AndroidX.Core.App;
using CalendarScraber.Models;
using CalendarScraber.Services;
using Application = Android.App.Application;

namespace CalendarScraber;

public class SystemAlarmService : ISystemAlarmService
{
    public void SetAlarm(DateTime alarmTime, CalendarView ev)
    {
        var calendar = Java.Util.Calendar.Instance;
        calendar.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
        calendar.Set(
            alarmTime.Year, 
            alarmTime.Month - 1, 
            alarmTime.Day, 
            alarmTime.Hour, 
            alarmTime.Minute, 
            0
        );
        calendar.Set(Java.Util.CalendarField.Millisecond, 0);

        AppLogger.Log($"Android SetAlarm: {alarmTime:dd.MM.yyyy HH:mm} (Java Millis: {calendar.TimeInMillis})");
        
        var intent = new Intent(Application.Context, typeof(AlarmReceiver));
        // Кладем только JSON. ID нам тут нужен только для RequestCode
        intent.PutExtra("event_json", JsonSerializer.Serialize(ev));

        // Генерируем уникальный код из ID, чтобы будильники не перезатирали друг друга
        var requestCode = ev.ItemId.Id.GetHashCode();

        var pendingIntent = PendingIntent.GetBroadcast(
            Application.Context, 
            requestCode, 
            intent, 
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
        )!;

        var manager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService)!;
        var alarmClockInfo = new AlarmManager.AlarmClockInfo(calendar.TimeInMillis, pendingIntent);
        manager.SetAlarmClock(alarmClockInfo, pendingIntent);
        
        AppLogger.Log($"Android AlarmManager: установлен ID={ev.ItemId.Id.GetHashCode()}");
    }

    public void CancelAlarm(CalendarView ev)
    {
        var manager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService)!;
        var intent = new Intent(Application.Context, typeof(AlarmReceiver));
        var requestCode = ev.ItemId.Id.GetHashCode();
        
        var pendingIntent = PendingIntent.GetBroadcast(
            Application.Context, requestCode, intent, 
            PendingIntentFlags.Immutable | PendingIntentFlags.NoCreate);

        if (pendingIntent != null)
        {
            manager.Cancel(pendingIntent);
            pendingIntent.Cancel();
        }
        
        AppLogger.Log($"Android AlarmManager: отменен ID={ev.ItemId.Id.GetHashCode()}");
    }
    
    public void CancelNotification(string eventId)
    {
        // Вычисляем тот же ID, что и при создании уведомления
        var notificationId = eventId.GetHashCode();

        var manager = NotificationManagerCompat.From(Application.Context);
        manager?.Cancel(notificationId);
        
        AppLogger.Log($"Уведомление убрано для {eventId.GetHashCode()}");
    }
}