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
    public void SetAlarm(int hour, int minute, CalendarView ev)
    {
        var calendar = Java.Util.Calendar.Instance;
        calendar.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
        calendar.Set(Java.Util.CalendarField.HourOfDay, hour);
        calendar.Set(Java.Util.CalendarField.Minute, minute);
        calendar.Set(Java.Util.CalendarField.Second, 0);

        // Если время прошло, будильник сработает сразу (или можно добавить день)
        
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
    }
    
    public void CancelNotification(string eventId)
    {
        // Вычисляем тот же ID, что и при создании уведомления
        var notificationId = eventId.GetHashCode();

        var manager = NotificationManagerCompat.From(Application.Context);
        manager?.Cancel(notificationId);
    }
}