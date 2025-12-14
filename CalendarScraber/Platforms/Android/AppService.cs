using Android.Content;
using CalendarScraber.Services;
using Android.Provider;
using Application = Android.App.Application;

namespace CalendarScraber;

public class AppService : IForegroundService
{
	public void Start(string title, string message)
	{
		CheckOverlayPermission();

		var intent = new Intent(Application.Context, typeof(ForegroundEventService));
		intent.PutExtra("title", title);
		intent.PutExtra("message", message);

		Application.Context.StartForegroundService(intent);
	}

	private void CheckOverlayPermission()
	{
		if (!Settings.CanDrawOverlays(Application.Context))
		{
			// Если разрешения нет - отправляем пользователя в настройки
			var intent = new Intent(Settings.ActionManageOverlayPermission,
				Android.Net.Uri.Parse("package:" + Application.Context.PackageName));
			intent.AddFlags(ActivityFlags.NewTask);
			Application.Context.StartActivity(intent);
		}
	}

	public void Stop()
	{
		var intent = new Intent(Application.Context, typeof(ForegroundEventService));
		Application.Context.StopService(intent);
	}
}