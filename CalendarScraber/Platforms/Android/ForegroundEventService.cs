using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Android.Content.PM;
using CalendarScraber.Models;
using CalendarScraber.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace CalendarScraber;

[Service(ForegroundServiceType = ForegroundService.TypeSystemExempted, Exported = false)]
public class ForegroundEventService : Service
{
	private bool _isRunning;
	private CancellationTokenSource? _cts;
	public bool HasAuthToken { get; set; }

	public override IBinder? OnBind(Intent? intent) => null;

	public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
	{
		// Если сервис уже работает, не перезапускаем логику, только обновляем уведомление если надо
		if (_isRunning)
		{
			AppLogger.Log("🛡️ Service: Уже запущен, игнорируем повторный старт.");
			return StartCommandResult.Sticky;
		}

		_isRunning = true;
		_cts = new();

		var title = intent?.GetStringExtra("title") ?? "Календарь";

		AppLogger.Log($"🛡️ ForegroundService: ЗАПУСК ФОНОВОГО ПРОЦЕССА. Title='{title}'");

		// 1. Показываем уведомление, чтобы Android не убил нас
		StartForegroundNotification(title, "Ожидание данных...");

		RegisterUpdate();

		// 2. Запускаем бесконечный цикл обновления в отдельном потоке
		Task.Run(async () => await UpdateLoopAsync(_cts.Token));

		return StartCommandResult.Sticky;
	}

	public void RegisterUpdate()
	{
		WeakReferenceMessenger.Default.Register<UpdateMessage>(this, (r, m) =>
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				AppLogger.Log("🛡️ ForegroundService: Получен запрос на обновление");
				_ = UpdateAsync();
			});
		});
	}

	private async Task UpdateLoopAsync(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			await UpdateAsync();

			// Ждем 1 минуту перед следующим обновлением
			AppLogger.Log("💤 Service: Сон 60 сек...");
			await Task.Delay(60000, token);
		}
	}

	private async Task UpdateAsync()
	{
		var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
		var calendarService = services?.GetService<CalendarService>();
		var alarmService = services?.GetService<AlarmService>();
		try
		{
			AppLogger.Log("🔄 Service: Начало цикла проверки...");

			if (calendarService != null && alarmService != null)
			{
				await RestoreSession(calendarService);

				var events = await calendarService.GetEventsAsync();

				if (events != null)
				{
					AppLogger.Log($"✅ Service: Получено {events.Count} событий.");

					// 2. ОБНОВЛЕНИЕ БУДИЛЬНИКОВ
					alarmService.ScheduleSystemAlarms(events);

					// 3. ОТПРАВКА ДАННЫХ В UI (если приложение открыто)
					WeakReferenceMessenger.Default.Send(new EventsUpdatedMessage(events));

					// 4. ОБНОВЛЕНИЕ ТЕКСТА В ШТОРКЕ
					UpdateNotificationShade(events);
				}
				else
				{
					AppLogger.Log("⚠️ Service: Не удалось получить события (null).");
				}
			}
		}
		catch (UnauthorizedAccessException)
		{
			AppLogger.Log("🔒 Service: Ошибка 401. Требуется авторизация!");

			// 1. МЕНЯЕМ УВЕДОМЛЕНИЕ В ШТОРКЕ
			// Чтобы пользователь видел, почему данные не идут
			var notificationManager = GetSystemService(NotificationService) as NotificationManager;
			var notification = CreateNotification("Календарь", "⚠️ Требуется авторизация. Нажмите для входа.");
			notificationManager?.Notify(GetHashCode(), notification);

			HasAuthToken = false;
			calendarService?.UpdateCookies(new());

			// 2. ОТПРАВЛЯЕМ СООБЩЕНИЕ В UI (Если приложение открыто)
			WeakReferenceMessenger.Default.Send(new LoginRequiredMessage());
		}
		catch (Exception ex)
		{
			AppLogger.Log($"❌ Service Error: {ex.Message}");
		}
	}

	private async Task RestoreSession(CalendarService calendarService)
	{
		if (HasAuthToken)
		{
			return;
		}

		var savedCookies = await CookieStorage.LoadCookies();

		var cookiesCollection = savedCookies.GetCookies(new(AppConfig.BaseDomain));

		foreach (Cookie c in cookiesCollection)
		{
			if (c.Name.Equals(AppConfig.AuthCookieName, StringComparison.OrdinalIgnoreCase))
			{
				HasAuthToken = true;
				break;
			}
		}

		if (HasAuthToken)
		{
			calendarService.UpdateCookies(savedCookies);
			AppLogger.Log("Session restored from storage.");
		}
	}

	private void UpdateNotificationShade(List<CalendarView> events)
	{
		var now = DateTime.UtcNow;
		var nextEvent = events
			.Where(e => e.Start > now && !e.IsCancelled)
			.MinBy(e => e.Start);

		Notification notification;
		if (nextEvent != null)
		{
			var title = $"Ближайшее: {nextEvent.LocalStart:HH:mm}";
			notification = CreateNotification(title, nextEvent.DisplaySubject);
			AppLogger.Log($"🔔 Обновлена шторка: {nextEvent.DisplaySubject}");
		}
		else
		{
			notification = CreateNotification("Календарь", "Нет предстоящих событий");
			AppLogger.Log("🔔 Обновлена шторка: Нет предстоящих событий");
		}

		// Обновляем уведомление
		var notificationManager = GetSystemService(NotificationService) as NotificationManager;
		notificationManager?.Notify(GetHashCode(), notification);
	}

	private void StartForegroundNotification(string title, string body)
	{
		CreateNotificationChannel();
		var notification = CreateNotification(title, body);

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
	}

	private Notification CreateNotification(string title, string body)
	{
		var pendingIntent = PendingIntent.GetActivity(this, 0, new(this, typeof(MainActivity)),
			PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

		return new NotificationCompat.Builder(this, "fg_channel_id")
			.SetContentTitle(title)
			?.SetContentText(body)
			?.SetSmallIcon(Android.Resource.Drawable.IcMenuMyCalendar)
			?.SetContentIntent(pendingIntent)
			?.SetOngoing(true)
			?.SetOnlyAlertOnce(true) // Чтобы не пиликало при каждом обновлении текста
			?.Build()!;
	}

	public override void OnDestroy()
	{
		AppLogger.Log("🛑 ForegroundService: Служба уничтожается (OnDestroy)");
		_isRunning = false;
		_cts?.Cancel();
		base.OnDestroy();
	}

	private void CreateNotificationChannel()
	{
		// Проверка на API 26 не нужна, так как у нас API 29+
		var channel = new NotificationChannel("fg_channel_id", "Фоновая служба", NotificationImportance.Low);
		var manager = GetSystemService(NotificationService) as NotificationManager;
		manager?.CreateNotificationChannel(channel);
	}
}