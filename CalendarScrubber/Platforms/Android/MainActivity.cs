using System.Text.Json;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using CalendarScrubber.Services;

namespace CalendarScrubber;

using CalendarView = Models.CalendarView;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
	ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
		ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	public Intent? NewIntent { get; set; }

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		AppLogger.Log("📱 MainActivity: OnCreate");
	}
	
	protected override void OnResume()
	{
		base.OnResume();
		ProcessIncomingIntent(NewIntent);
	}

	protected override void OnNewIntent(Intent? intent)
	{
		base.OnNewIntent(intent);
		AppLogger.Log("📨 MainActivity: OnNewIntent получен новый интент");
		NewIntent = intent;
	}

	private void ProcessIncomingIntent(Intent? intent)
	{
		TurnOnScreen();

		if (intent != null && intent.HasExtra("trigger_json"))
		{
			var json = intent.GetStringExtra("trigger_json");
			
			AppLogger.Log("🎯 MainActivity: Найден ключ 'trigger_json'. Обработка...");

			if (!string.IsNullOrEmpty(json))
			{
				try
				{
					// ДЕСЕРИАЛИЗАЦИЯ ПРЯМО ЗДЕСЬ
					var eventData = JsonSerializer.Deserialize<CalendarView>(json);

					if (eventData != null)
					{
						AppLogger.Log($"🚀 Открытие AlarmPage для: {eventData.Subject}");
						
						MainThread.BeginInvokeOnMainThread(() =>
						{
							Shell.Current?.Navigation.PushModalAsync(new Pages.AlarmPage(eventData));
							NewIntent = null;
						});
					}
				}
				catch (Exception ex)
				{
					AppLogger.Log($"❌ MainActivity Ошибка десериализации: {ex.Message}");
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