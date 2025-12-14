using System.Text.Json;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using CalendarScraber.Models;

namespace CalendarScraber;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class AlarmReceiver : BroadcastReceiver
{
	public override void OnReceive(Context? context, Intent? intent)
	{
		if (context == null || intent == null) return;

		var pm = (PowerManager)context.GetSystemService(Context.PowerService)!;
		var wakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "CalendarScraber:AlarmWakeLock");
		wakeLock?.Acquire(5 * 1000L);

		try
		{
			var json = intent.GetStringExtra("event_json") ?? "";
			if (string.IsNullOrEmpty(json)) return;
			var isScreenOn = pm.IsInteractive;

			ShowNotification(context, json, fullScreen: !isScreenOn);
		}
		finally
		{
			wakeLock?.Release();
		}
	}

	private void ShowNotification(Context context, string json, bool fullScreen)
	{
		var ev = JsonSerializer.Deserialize<CalendarView>(json);
		if (ev == null) return;

		var subject = ev.DisplaySubject;
		var time = $"{ev.LocalStart:HH:mm}";
		var notificationId = ev.ItemId.Id.GetHashCode();

		var activityIntent = new Intent(context, typeof(MainActivity));
		activityIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
		activityIntent.PutExtra("trigger_json", json);

		var pendingIntent = PendingIntent.GetActivity(
			context,
			notificationId,
			activityIntent,
			PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

		var channelId = "alarm_critical_channel";
		CreateNotificationChannel(context, channelId);

		var notificationBuilder = new NotificationCompat.Builder(context, channelId)
			.SetSmallIcon(Android.Resource.Drawable.IcMenuMyCalendar)
			?.SetContentTitle($"⏰ {time} {subject}")
			?.SetContentText("Нажмите, чтобы открыть")
			?.SetPriority(NotificationCompat.PriorityMax)
			?.SetCategory(NotificationCompat.CategoryAlarm)
			?.SetAutoCancel(true);

		if (fullScreen)
		{
			notificationBuilder?.SetFullScreenIntent(pendingIntent, highPriority: true);
		}
		else
		{
			notificationBuilder?.SetContentIntent(pendingIntent);
		}

		var notificationManager = NotificationManagerCompat.From(context);
		notificationManager!.Notify(notificationId, notificationBuilder!.Build());
	}

	private void CreateNotificationChannel(Context context, string channelId)
	{
		if (context.GetSystemService(Context.NotificationService) is not NotificationManager manager) return;

		if (manager.GetNotificationChannel(channelId) != null) return;

		var channel = new NotificationChannel(channelId, "Будильник", NotificationImportance.High)
		{
			Importance = NotificationImportance.High,
			LockscreenVisibility = NotificationVisibility.Public,
		};


		channel.SetSound(null, null);

		manager.CreateNotificationChannel(channel);
	}
}