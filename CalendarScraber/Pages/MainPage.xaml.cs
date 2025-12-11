using CalendarScraber.Services;

namespace CalendarScraber.Pages;

public partial class MainPage : ContentPage
{
	private CalendarService _calendarService = new CalendarService();
	private System.Timers.Timer _timer;

	public MainPage()
	{
		InitializeComponent();
        
		// Настраиваем таймер (например, каждые 60 секунд)
		_timer = new System.Timers.Timer(60000);
		_timer.Elapsed += async (s, e) => await CheckCalendar();
	}

	// Кнопка "Войти" на главном экране
	private async void OnLoginClicked(object sender, EventArgs e)
	{
		var loginPage = Handler!.MauiContext!.Services.GetService<LoginPage>();
		loginPage.OnLoginSuccess += (cookies) =>
		{
			// 1. Инициализируем HTTP клиент с полученными куками
			_calendarService.InitializeClient(cookies);
            
			// 2. Запускаем таймер проверки
			_timer.Start();
            
			// 3. Делаем первую проверку сразу
			Task.Run(CheckCalendar);
		};

		await Navigation.PushModalAsync(loginPage);
	}

	private async Task CheckCalendar()
	{
		// Показываем спиннер, что идет запрос
		MainThread.BeginInvokeOnMainThread(() => LoadingSpinner.IsRunning = true);

		var events = await _calendarService.CheckEventsAsync();

		MainThread.BeginInvokeOnMainThread(() =>
		{
			LoadingSpinner.IsRunning = false;
        
			if (events != null)
			{
				StatusLabel.Text = $"Обновлено: {DateTime.Now:HH:mm:ss} (Найдено: {events.Count})";
				StatusLabel.TextColor = Colors.Green;

				// ! ВАЖНО: Заполняем CollectionView данными
				EventsCollection.ItemsSource = events; 
			}
			else
			{
				StatusLabel.Text = "Ошибка получения данных";
				StatusLabel.TextColor = Colors.Red;
			}
		});
	}
}