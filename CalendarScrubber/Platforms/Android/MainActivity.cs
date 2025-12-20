using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using CalendarScrubber.Models;
using CalendarScrubber.Services;

namespace CalendarScrubber;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
	ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
		ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	private IEventStorage? _eventStorage;
	public Intent? NewIntent { get; set; }

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		AppLogger.Log("📱 MainActivity: OnCreate");
		var service = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
		_eventStorage = service?.GetService<IEventStorage>();
	}
	
	protected override async void OnResume()
	{
		base.OnResume();
		await ProcessIncomingIntent(NewIntent);
	}

	protected override void OnNewIntent(Intent? intent)
	{
		base.OnNewIntent(intent);
		AppLogger.Log("📨 MainActivity: OnNewIntent получен новый интент");
		NewIntent = intent;
	}

	private async Task ProcessIncomingIntent(Intent? intent)
	{
		TurnOnScreen();

		if (intent != null && intent.HasExtra("trigger_id"))
		{
			var id = intent.GetStringExtra("trigger_id");
			
			AppLogger.Log("🎯 MainActivity: Найден ключ 'trigger_id'. Обработка...");

			if (!string.IsNullOrEmpty(id))
			{
				try
				{
					// ДЕСЕРИАЛИЗАЦИЯ ПРЯМО ЗДЕСЬ
					var eventData = await (_eventStorage?.GetEventAsync(id) ?? Task.FromResult<CalendarView?>(null));

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