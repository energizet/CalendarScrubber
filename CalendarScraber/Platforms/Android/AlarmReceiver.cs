using System.Text.Json;
using _Microsoft.Android.Resource.Designer;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using CalendarScraber.Models;
using CalendarScraber.Services;
using Application = Microsoft.Maui.Controls.Application;

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
			
			var soundPlayer = Application.Current?.Handler?.MauiContext?.Services.GetService<ISystemSoundPlayer>();
			soundPlayer?.Play();

			ShowNotification(context, json);
		}
		finally
		{
			wakeLock?.Release();
		}
	}

	private void ShowNotification(Context context, string json)
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

		// === Интент для кнопки "СТОП" ===
		var stopIntent = new Intent(context, typeof(StopAlarmReceiver));
		stopIntent.PutExtra("notification_id", ev.ItemId.Id);
        
		var stopPendingIntent = PendingIntent.GetBroadcast(
			context,
			notificationId,
			stopIntent,
			PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
		
		var channelId = "alarm_critical_channel";
		CreateNotificationChannel(context, channelId);

		var packageName = context.PackageName;
		var remoteViews = new Android.Widget.RemoteViews(packageName, ResourceConstant.Layout.notification_custom);

		// Привязываем данные к элементам XML
		remoteViews.SetTextViewText(ResourceConstant.Id.txt_time, time);
		remoteViews.SetTextViewText(ResourceConstant.Id.txt_subject, subject);

		var notificationBuilder = new NotificationCompat.Builder(context, channelId)
			.SetSmallIcon(Android.Resource.Drawable.IcMenuMyCalendar)
			?.SetCustomContentView(remoteViews)
			?.SetPriority(NotificationCompat.PriorityMax)
			?.SetCategory(NotificationCompat.CategoryAlarm)
			?.SetAutoCancel(true)
			?.SetOngoing(true)
			?.SetVisibility(NotificationCompat.VisibilityPublic)
			?.SetContentIntent(stopPendingIntent)
			?.SetFullScreenIntent(pendingIntent, highPriority: true);

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