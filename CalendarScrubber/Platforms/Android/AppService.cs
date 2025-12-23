using Android.Content;
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

		AppLogger.Log($"🚀 AppService: Запрос запуска. Title='{title}'");

		var intent = new Intent(Application.Context, typeof(ForegroundEventService));
		intent.PutExtra("title", title);
		intent.PutExtra("message", message);

		Application.Context.StartForegroundService(intent);

		AppLogger.Log("✅ AppService: StartForegroundService отправлен");
	}

	public void Stop()
	{
		AppLogger.Log("🛑 AppService: Запрос остановки сервиса");
		var intent = new Intent(Application.Context, typeof(ForegroundEventService));
		Application.Context.StopService(intent);
	}
}