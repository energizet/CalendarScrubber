using CalendarScraber.Services;

namespace CalendarScraber.Pages;

public partial class MainPage : ContentPage
{
    private readonly CalendarService _calendarService;
    private readonly IServiceProvider _serviceProvider;
    private System.Timers.Timer _timer;
    
    // Флаг, чтобы не открыть 10 окон авторизации, если таймер тикает
    private bool _isLoginOpen = false; 

    public MainPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _calendarService = new CalendarService(); // Создаем "пустой" сервис

        // Настраиваем таймер (раз в минуту)
        _timer = new System.Timers.Timer(60000);
        _timer.Elapsed += async (s, e) => await LoadDataAsync();
    }

    // Метод вызывается при старте приложения
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _timer.Start();
        
        // Сразу пробуем загрузить данные "не думая"
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        // Если окно логина уже открыто, не долбим запросами
        if (_isLoginOpen) return;

        try
        {
            MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = "Проверка...");

            // 1. Делаем запрос
            var events = await _calendarService.GetEventsAsync();

            // 2. Если успех - обновляем UI
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (events != null)
                {
                    EventsCollection.ItemsSource = events;
                    StatusLabel.Text = $"Обновлено: {DateTime.UtcNow.ToLocalTime():HH:mm}";
                }
            });
        }
        catch (UnauthorizedAccessException)
        {
            // 3. ПОЙМАЛИ 401 -> ЗАПУСКАЕМ АВТОРИЗАЦИЮ
            await OpenLoginModal();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async Task OpenLoginModal()
    {
        // Защита от открытия второго окна
        if (_isLoginOpen) return; 
        _isLoginOpen = true;

        // Переходим в главный поток, так как работаем с UI
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try 
            {
                // Получаем страницу через DI
                var loginPage = _serviceProvider.GetRequiredService<LoginPage>();

                // Подписываемся на успех
                loginPage.OnLoginSuccess += async (cookies) =>
                {
                    // 1. Обновляем куки в сервисе
                    _calendarService.UpdateCookies(cookies);
                    
                    // 2. Снимаем флаг блокировки
                    _isLoginOpen = false;

                    // 3. ПОВТОРЯЕМ ЗАПРОС СРАЗУ ЖЕ
                    StatusLabel.Text = "Вход выполнен. Обновление...";
                    await LoadDataAsync();
                };

                // Показываем окно
                await Navigation.PushModalAsync(loginPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка открытия окна: {ex}");
                _isLoginOpen = false;
            }
        });
    }
    
    // Обработчик нажатия кнопки из XAML
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Просто вызываем нашу логику открытия окна или загрузки данных
        // Если токена нет, OpenLoginModal вызовется внутри LoadDataAsync или можно вызвать напрямую
        await LoadDataAsync(); 
    }
}