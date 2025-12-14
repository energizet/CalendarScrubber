using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Android.Content.PM;
using CalendarScraber.Services;

namespace CalendarScraber;

[Service(ForegroundServiceType = ForegroundService.TypeSystemExempted, Exported = false)]
public class ForegroundEventService : Service
{
	public override IBinder? OnBind(Intent? intent) => null;

	public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
	{
		var title = intent?.GetStringExtra("title") ?? "Календарь";
		var message = intent?.GetStringExtra("message") ?? "Фоновая работа...";
		
		AppLogger.Log($"🛡️ ForegroundService: StartCommand. Title='{title}'");

		CreateNotificationChannel();

		// При нажатии открываем приложение
		var pendingIntent = PendingIntent.GetActivity(this, 0,
			new(this, typeof(MainActivity)), PendingIntentFlags.Immutable);

		var notification = new NotificationCompat.Builder(this, "fg_channel_id")
			.SetContentTitle(title)
			?.SetContentText(message)
			?.SetSmallIcon(Android.Resource.Drawable.IcMenuMyCalendar) // Или твоя иконка
			?.SetContentIntent(pendingIntent)
			?.SetOngoing(true)
			?.Build()!;

		// Запуск с типом systemExempted (для Android 14)
		if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake)
		{
#pragma warning disable CA1416
			StartForeground(GetHashCode(), notification, ForegroundService.TypeSystemExempted);
#pragma warning restore CA1416
		}
		else
		{
			StartForeground(GetHashCode(), notification);
		}

		return StartCommandResult.Sticky;
	}
	
	public override void OnDestroy()
	{
		base.OnDestroy();
		AppLogger.Log("🛑 ForegroundService: Служба уничтожается (OnDestroy)");
	}
	
	private void CreateNotificationChannel()
	{
		// Проверка на API 26 не нужна, так как у нас API 29+
		var channel = new NotificationChannel("fg_channel_id", "Фоновая служба", NotificationImportance.Low);
		var manager = GetSystemService(NotificationService) as NotificationManager;
		manager?.CreateNotificationChannel(channel);
	}
}