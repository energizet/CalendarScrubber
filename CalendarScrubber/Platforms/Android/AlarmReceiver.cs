using _Microsoft.Android.Resource.Designer;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using CalendarScrubber.Models;
using CalendarScrubber.Services;
using Application = Microsoft.Maui.Controls.Application;

namespace CalendarScrubber;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class AlarmReceiver : BroadcastReceiver
{
	private ISystemSoundPlayer? _soundPlayer;
	private IEventStorage? _eventStorage;

	public override async void OnReceive(Context? context, Intent? intent)
	{
		if (context == null || intent == null) return;

		var pm = (PowerManager)context.GetSystemService(Context.PowerService)!;
		var wakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "CalendarScrubber:AlarmWakeLock");
		wakeLock?.Acquire(5 * 1000L);

		try
		{
			var id = intent.GetStringExtra("event_id") ?? "";
			if (string.IsNullOrEmpty(id)) return;

			var service = Application.Current?.Handler?.MauiContext?.Services;
			_eventStorage ??= service?.GetService<IEventStorage>();
			var ev = await (_eventStorage?.GetEventAsync(id) ?? Task.FromResult<CalendarView?>(null));
			if (ev == null) return;

			_soundPlayer ??= service?.GetService<ISystemSoundPlayer>();
			_soundPlayer?.Play();
			AppLogger.Log("🎵 Звук запущен");

			ShowNotification(context, ev);
		}
		finally
		{
			wakeLock?.Release();
		}
	}

	private static void ShowNotification(Context context, CalendarView ev)
	{
		AppLogger.Log($"⏰ СРАБОТАЛ БУДИЛЬНИК: {ev.Subject}");

		var subject = ev.DisplaySubject;
		var time = $"{ev.LocalStart:HH:mm}";
		var notificationId = ev.ItemId.Id.GetHashCode();

		var activityIntent = new Intent(context, typeof(MainActivity));
		activityIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
		activityIntent.PutExtra("trigger_id", ev.ItemId.Id);

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

	private static void CreateNotificationChannel(Context context, string channelId)
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