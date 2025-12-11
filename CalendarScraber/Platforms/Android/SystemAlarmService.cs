using Android.Content;
using Android.Provider;
using CalendarScraber.Services;

namespace CalendarScraber;

public class SystemAlarmService : ISystemAlarmService
{
	public void SetAlarm(int hour, int minute, string title)
	{
		try
		{
			var intent = new Intent(AlarmClock.ActionSetAlarm);
            
			// Устанавливаем параметры
			intent.PutExtra(AlarmClock.ExtraHour, hour);
			intent.PutExtra(AlarmClock.ExtraMinutes, minute);
			intent.PutExtra(AlarmClock.ExtraMessage, title);
            
			// ExtraSkipUi = true означает, что будильник поставится "тихо", 
			// не открывая приложение Часы перед пользователем (если система позволяет)
			intent.PutExtra(AlarmClock.ExtraSkipUi, true);

			// Флаг нужен, так как мы запускаем из контекста приложения
			intent.AddFlags(ActivityFlags.NewTask);

			// Проверяем, есть ли приложение, способное обработать этот интент (приложение Часы)
			if (intent.ResolveActivity(Android.App.Application.Context.PackageManager!) != null)
			{
				Android.App.Application.Context.StartActivity(intent);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("Не найдено приложение Часов");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Ошибка установки будильника: {ex.Message}");
		}
	}
}