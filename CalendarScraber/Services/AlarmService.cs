using CalendarScraber.Models;
using Plugin.LocalNotification;

namespace CalendarScraber.Services;

public class AlarmService(ISystemAlarmService systemAlarmService)
{
	// Этот список нужен, чтобы не ставить будильник на одно событие 100 раз
	// Можно хранить ID событий в Preferences, если нужно сохранять между перезапусками
	private HashSet<string> _scheduledEvents = new HashSet<string>();

	public void ScheduleSystemAlarms(List<CalendarView> events)
	{
		if (!SettingsManager.IsAlarmEnabled) return;

		var now = DateTime.Now; // Локальное время
		var minutesThreshold = SettingsManager.MinutesBefore; // Например, 15 мин

		foreach (var ev in events)
		{
			// Уникальный ID для отслеживания
			if (_scheduledEvents.Contains(ev.ItemId.Id)) continue;

			// Пропускаем отмененные
			if (SettingsManager.OnlyActiveEvents && (ev.IsCancelled
				    || ev.Status == "NoResponseReceived"
				    || ev.Status == "Tentative"
			    )) continue;

			// Вычисляем время, когда должен зазвенеть будильник
			// LocalStart (10:00) - 15 минут = 09:45
			var alarmTime = ev.LocalStart.AddMinutes(-minutesThreshold);

			// Если время будильника уже прошло - пропускаем
			if (alarmTime < now) continue;

			// Если до будильника осталось больше 24 часов - тоже можно пропустить пока
			if ((alarmTime - now).TotalHours > 24) continue;

			// === УСТАНАВЛИВАЕМ БУДИЛЬНИК ===
			systemAlarmService.SetAlarm(
				alarmTime.Hour,
				alarmTime.Minute,
				$"Встреча: {ev.Subject}"
			);

			// Запоминаем, что мы уже поставили будильник на это событие
			_scheduledEvents.Add(ev.ItemId.Id);

			System.Diagnostics.Debug.WriteLine($"Будильник установлен на {alarmTime:HH:mm} для {ev.Subject}");
		}
	}

	public async Task CheckAndTriggerAlarmAsync(List<CalendarView>? events)
	{
		// 1. Если будильник выключен - выходим
		if (!SettingsManager.IsAlarmEnabled || events == null || events.Count == 0) return;

		var nowUtc = DateTime.UtcNow;
		var minutesThreshold = SettingsManager.MinutesBefore;
		var onlyActive = SettingsManager.OnlyActiveEvents;

		// 2. Ищем ближайшее подходящее событие
		foreach (var ev in events)
		{
			// Фильтрация: Пропускаем, если событие уже прошло
			if (ev.Start < nowUtc) continue;

			// Фильтрация по статусу (если включена настройка)
			if (onlyActive)
			{
				if (ev.IsCancelled) continue;
				if (ev.Status == "NoResponseReceived") continue; // Или ваша логика активного события
				if (ev.Status == "Tentative") continue; // Или ваша логика активного события
			}

			// 3. Вычисляем, сколько осталось
			var timeUntilEvent = ev.Start - nowUtc;

			// 4. Проверка: Если осталось меньше N минут, но больше 0 (не прошлое)
			if (timeUntilEvent.TotalMinutes <= minutesThreshold && timeUntilEvent.TotalMinutes > -1)
			{
				// ЗАЩИТА ОТ ДУБЛЕЙ: 
				// Проверяем, не звонили ли мы уже по этому конкретному ID
				if (SettingsManager.LastTriggeredEventId != ev.ItemId.Id)
				{
					// 5. Звоним!
					await SendNotification(ev);

					// Запоминаем, что для этого события будильник отработал
					SettingsManager.LastTriggeredEventId = ev.ItemId.Id;
				}

				// Нашли ближайшее, оповестили (или пропустили если уже было), дальше искать нет смысла
				// (если нужно оповещать о нескольких событиях одновременно, уберите break)
				break;
			}
		}
	}

	public async Task ForceTriggerAlarmAsync(CalendarView ev)
	{
		// Просто сразу вызываем отправку уведомления
		// Можно добавить пометку "(Тест)" в заголовок, чтобы отличать
		await SendNotification(ev, isTest: true);
	}

	private async Task SendNotification(CalendarView ev, bool isTest = false)
	{
		var title = isTest ? "[ТЕСТ] Напоминание" : "Напоминание о встрече";

		var request = new NotificationRequest
		{
			NotificationId = new Random().Next(1000, 9999), // Случайный ID, чтобы уведомления не затирали друг друга
			Title = title,
			Description = $"{ev.Subject} (Начало: {ev.LocalStart:HH:mm})",
			BadgeNumber = 1,
			Schedule = new NotificationRequestSchedule
			{
				NotifyTime = DateTime.Now.AddSeconds(1) // Сработать мгновенно
			}
		};

#if ANDROID
        await LocalNotificationCenter.Current.Show(request);
#endif
	}
}