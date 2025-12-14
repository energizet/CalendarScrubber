using System.Text.Json;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Views;
using CalendarView = CalendarScraber.Models.CalendarView;

namespace CalendarScraber;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
	ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
		ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	public Intent? NewIntent { get; set; }

	protected override void OnResume()
	{
		base.OnResume();
		ProcessIncomingIntent(NewIntent);
	}

	protected override void OnNewIntent(Intent? intent)
	{
		base.OnNewIntent(intent);
		NewIntent = intent;
	}

	private void ProcessIncomingIntent(Intent? intent)
	{
		TurnOnScreen();

		if (intent != null && intent.HasExtra("trigger_json"))
		{
			var json = intent.GetStringExtra("trigger_json");

			if (!string.IsNullOrEmpty(json))
			{
				try
				{
					// ДЕСЕРИАЛИЗАЦИЯ ПРЯМО ЗДЕСЬ
					var eventData = JsonSerializer.Deserialize<CalendarView>(json);

					if (eventData != null)
					{
						MainThread.BeginInvokeOnMainThread(() =>
						{
							Shell.Current?.Navigation.PushModalAsync(new Pages.AlarmPage(eventData));
							NewIntent = null;
						});
						// Отправляем уже готовый объект!
						//WeakReferenceMessenger.Default.Send(new AlarmTriggeredMessage(eventData));
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Ошибка десериализации в MainActivity: {ex}");
				}
			}
		}
	}

	private void TurnOnScreen()
	{
		SetShowWhenLocked(true);
		SetTurnScreenOn(true);

		// Дополнительные флаги окна для надежности
		Window?.AddFlags(
			WindowManagerFlags.ShowWhenLocked |
			WindowManagerFlags.DismissKeyguard |
			WindowManagerFlags.KeepScreenOn |
			WindowManagerFlags.TurnScreenOn |
			WindowManagerFlags.AllowLockWhileScreenOn);

		//var keyguardManager = (KeyguardManager?)GetSystemService(KeyguardService);
		//keyguardManager?.RequestDismissKeyguard(this, null);
	}
}