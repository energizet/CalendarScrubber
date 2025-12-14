using System.Text.Json;
using Android.Content;
using CalendarScraber.Models;
using CalendarScraber.Services;
using Application = Microsoft.Maui.Controls.Application;

namespace CalendarScraber;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class StopAlarmReceiver : BroadcastReceiver
{
	public override void OnReceive(Context? context, Intent? intent)
	{
		if (context == null) return;

		try
		{
			AppLogger.Log("🔔 Нажата кнопка СТОП в уведомлении");
			
			var soundPlayer = Application.Current?.Handler?.MauiContext?.Services.GetService<ISystemSoundPlayer>();
			soundPlayer?.Stop();

			var id = intent?.GetStringExtra("notification_id") ?? "";
			var alarmService = Application.Current?.Handler?.MauiContext?.Services.GetService<ISystemAlarmService>();
			alarmService?.CancelNotification(id);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Ошибка остановки звука: {ex}");
		}
	}
}