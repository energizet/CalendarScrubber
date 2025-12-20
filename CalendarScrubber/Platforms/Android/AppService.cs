using Android.Content;
using Android.Provider;
using CalendarScrubber.Services;
using Plugin.LocalNotification;
using Application = Android.App.Application;

namespace CalendarScrubber;

public class AppService : IForegroundService
{
	public async Task Start(string title, string message)
	{
		if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
		{
			await LocalNotificationCenter.Current.RequestNotificationPermission();
		}

		CheckOverlayPermission();

		AppLogger.Log($"🚀 AppService: Запрос запуска. Title='{title}'");

		var intent = new Intent(Application.Context, typeof(ForegroundEventService));
		intent.PutExtra("title", title);
		intent.PutExtra("message", message);

		Application.Context.StartForegroundService(intent);

		AppLogger.Log("✅ AppService: StartForegroundService отправлен");
	}

	private void CheckOverlayPermission()
	{
		if (!Settings.CanDrawOverlays(Application.Context))
		{
			AppLogger.Log("⚠️ AppService: Нет прав на Overlay! Открываем настройки...");

			// Если разрешения нет - отправляем пользователя в настройки
			var intent = new Intent(Settings.ActionManageOverlayPermission,
				Android.Net.Uri.Parse("package:" + Application.Context.PackageName));
			intent.AddFlags(ActivityFlags.NewTask);
			Application.Context.StartActivity(intent);
		}
	}

	public void Stop()
	{
		AppLogger.Log("🛑 AppService: Запрос остановки сервиса");
		var intent = new Intent(Application.Context, typeof(ForegroundEventService));
		Application.Context.StopService(intent);
	}
}