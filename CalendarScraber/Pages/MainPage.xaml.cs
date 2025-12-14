using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using CalendarScraber.Models;
using CalendarScraber.Services;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.LocalNotification;

namespace CalendarScraber.Pages;

public partial class MainPage : ContentPage
{
	private readonly CalendarService _calendarService;
	private readonly AlarmService _alarmService;
	private readonly IServiceProvider _serviceProvider;
	private readonly IForegroundService _foregroundService;

	public ObservableCollection<AppLog> Logs { get; set; } = [];

	private bool _isLoginOpen = false;

	public MainPage(IServiceProvider serviceProvider)
	{
		InitializeComponent();
#if DEBUG
		//RunBtn.IsVisible = true;
#endif
		_serviceProvider = serviceProvider;
		_calendarService = _serviceProvider.GetRequiredService<CalendarService>();
		_alarmService = _serviceProvider.GetRequiredService<AlarmService>();
		_foregroundService = _serviceProvider.GetRequiredService<IForegroundService>();

		RegisterLog();
		RegisterUpdate();
		RegisterLogin();
	}

	private async void OnSettingsClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new SettingsPage());
	}


	protected override async void OnAppearing()
	{
		base.OnAppearing();

		_foregroundService.Start("Календарь", "Запуск мониторинга...");
	}

	private async Task LoadDataAsync()
	{
		if (_isLoginOpen)
		{
			AppLogger.Log("⏸ Обновление пропущено: открыто окно логина");
			return;
		}

		try
		{
			MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = "Проверка...");

			var events = await _calendarService.GetEventsAsync();

			MainThread.BeginInvokeOnMainThread(() =>
			{
				if (events != null)
				{
					AppLogger.Log("🎨 Обновление UI списка событий...");
					EventsCollection.ItemsSource = events;
					StatusLabel.Text = $"Обновлено: {DateTime.UtcNow.ToLocalTime():HH:mm}";

					Task.Run(() =>
					{
						_alarmService.ScheduleSystemAlarms(events);
						//await _alarmService.CheckAndTriggerAlarmAsync(events);
					});
				}
			});
		}
		catch (UnauthorizedAccessException)
		{
			AppLogger.Log("🔒 Поймано исключение 401. Открываем вход...");
			await OpenLoginModal();
		}
		catch (Exception ex)
		{
			AppLogger.Log($"💥 Критическая ошибка в LoadData: {ex.Message}");
			Debug.WriteLine(ex);
		}
	}

	private async Task OpenLoginModal()
	{
		if (_isLoginOpen) return;
		_isLoginOpen = true;


		await MainThread.InvokeOnMainThreadAsync(async () =>
		{
			try
			{
				AppLogger.Log("🔑 Открытие окна авторизации");
				var loginPage = _serviceProvider.GetRequiredService<LoginPage>();

				loginPage.OnLoginSuccess += async (cookies) =>
				{
					AppLogger.Log("✅ LoginSuccess сработал. Сохраняем куки...");
					_calendarService.UpdateCookies(cookies);
					_isLoginOpen = false;

					AppLogger.Log("🔄 Повторный запрос данных после входа...");
					StatusLabel.Text = "Вход выполнен. Обновление...";
					await LoadDataAsync();
				};


				await Navigation.PushModalAsync(loginPage);
			}
			catch (Exception ex)
			{
				AppLogger.Log($"❌ Ошибка открытия LoginModal: {ex}");
				Debug.WriteLine($"Ошибка открытия окна: {ex}");
				_isLoginOpen = false;
			}
		});
	}

	private void RunClicked(object sender, EventArgs e)
	{
		var id = Random.Shared.Next(0, 100);
		var ev = new CalendarView
		{
			Subject = "asd" + id,
			Start = DateTime.UtcNow.AddMinutes(2),
			End = DateTime.UtcNow.AddMinutes(10),
			ItemId = new()
			{
				Id = id.ToString(),
			},
		};
		var events = (List<CalendarView>)[ev];
		EventsCollection.ItemsSource = events;
		_alarmService.ScheduleSystemAlarms(events);
		//await _alarmService.CheckAndTriggerAlarmAsync(events);
	}

	private async void OnLoginClicked(object sender, EventArgs e)
	{
		await LoadDataAsync();
	}

	private void OnToggleLogsClicked(object sender, EventArgs e)
	{
		// 1. Инвертируем видимость
		LogsFrame.IsVisible = !LogsFrame.IsVisible;

		// 2. Меняем текст кнопки
		LogToggleBtn.Text = LogsFrame.IsVisible ? "🔽 Скрыть логи" : "📜 Показать логи";
	}

	private void RegisterLog()
	{
		LogsCollection.ItemsSource = Logs;
		WeakReferenceMessenger.Default.Register<LogTriggeredMessage>(this, (r, m) =>
		{
			var log = m.Value;

			MainThread.BeginInvokeOnMainThread(() =>
			{
				// Добавляем в НАЧАЛО списка, чтобы новые были сверху
				Logs.Insert(0, log);

				// Ограничим размер лога, чтобы память не текла (например, последние 100)
				if (Logs.Count > 100) Logs.RemoveAt(Logs.Count - 1);
			});
		});
	}

	private void RegisterLogin()
	{
		WeakReferenceMessenger.Default.Register<LoginRequiredMessage>(this, (r, m) =>
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				AppLogger.Log("🔑 MainPage: Получен запрос на вход от сервиса");
				_ = OpenLoginModal();
			});
		});
	}

	private void RegisterUpdate()
	{
		WeakReferenceMessenger.Default.Register<EventsUpdatedMessage>(this, (r, m) =>
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				var events = m.Value;
				EventsCollection.ItemsSource = events;
				StatusLabel.Text = $"Обновлено: {DateTime.Now:HH:mm}";
			});
		});
	}
}