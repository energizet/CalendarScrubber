using CalendarScrubber.Services;

namespace CalendarScrubber.Pages;

public partial class PermissionsPage : ContentPage
{
	private readonly IServiceProvider _serviceProvider;
	private readonly bool _isOnboarding;
	private readonly IPermissionHelper _permissionHelper;

	public PermissionsPage(IServiceProvider serviceProvider, bool isOnboarding = false)
	{
		InitializeComponent();
		_serviceProvider = serviceProvider;
		_isOnboarding = isOnboarding;
		_permissionHelper = serviceProvider.GetRequiredService<IPermissionHelper>();

		if (isOnboarding)
		{
			ContinueBtn.IsVisible = true;
			Title = "Начальная настройка";
			// Убираем кнопку "Назад" (стрелочку), чтобы пользователь обязательно прошел настройку
			NavigationPage.SetHasBackButton(this, false);
		}

		this.Loaded += OnPageLoaded;
		this.Unloaded += OnPageUnloaded;
	}

	// Срабатывает, когда страница полностью загружена и прикреплена к Окну
	private void OnPageLoaded(object? sender, EventArgs e)
	{
		// Теперь this.Window точно существует
		if (this.Window != null)
		{
			// Activated срабатывает, когда приложение возвращается из фона (из настроек)
			this.Window.Activated += OnWindowActivated;
		}
	}
	
	// Срабатывает, когда мы уходим со страницы (чистим память)
	private void OnPageUnloaded(object? sender, EventArgs e)
	{
		if (this.Window != null)
		{
			this.Window.Activated -= OnWindowActivated;
		}
	}

	private async void OnWindowActivated(object? sender, EventArgs e)
	{
		// Проверяем IsVisible, чтобы код выполнялся только если мы сейчас смотрим на эту страницу
		if (this.IsVisible)
		{
			// ВАЖНО: Даем Андроиду полсекунды на обновление статусов в системе
			await Task.Delay(500);
			await RefreshStatuses();
		}
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		await Task.Delay(200);
		await RefreshStatuses();
	}

	private async Task RefreshStatuses()
	{
		await MainThread.InvokeOnMainThreadAsync(async () =>
		{
			await UpdateCard(PermissionType.Notifications, StatusNotifications, FrameNotifications);
			await UpdateCard(PermissionType.Overlay, StatusOverlay, FrameOverlay);
			await UpdateCard(PermissionType.ExactAlarms, StatusAlarm, FrameAlarm);
			await UpdateCard(PermissionType.BatteryOptimization, StatusBattery, FrameBattery);
			await UpdateCard(PermissionType.FullScreenIntent, StatusFullScreen, FrameFullScreen);
		});
	}

	private async Task UpdateCard(PermissionType type, Label statusLabel, Frame frame)
	{
		var isGranted = await _permissionHelper.CheckStatusAsync(type);

		if (isGranted)
		{
			statusLabel.Text = "✅ Разрешено";
			statusLabel.TextColor = Colors.Green;
			frame.BorderColor = Colors.LightGreen;
			// Можно скрыть кнопку или сделать её неактивной, но лучше оставить
			// на случай, если пользователь захочет отозвать.
		}
		else
		{
			statusLabel.Text = "❌ Не разрешено";
			statusLabel.TextColor = Colors.Red;
			frame.BorderColor = Colors.Red;
		}
	}

	// Обработчики кнопок
	private async void OnRequestNotifications(object sender, EventArgs e)
	{
		await Request(PermissionType.Notifications);
	}

	private async void OnRequestOverlay(object sender, EventArgs e)
	{
		await Request(PermissionType.Overlay);
	}

	private async void OnRequestAlarm(object sender, EventArgs e)
	{
		await Request(PermissionType.ExactAlarms);
	}

	private async void OnRequestBattery(object sender, EventArgs e)
	{
		await Request(PermissionType.BatteryOptimization);
	}

	private async void OnRequestFullScreen(object sender, EventArgs e)
	{
		await Request(PermissionType.FullScreenIntent);
	}

	private async void OnRequestAutoStart(object sender, EventArgs e)
	{
		// Можно показать подсказку после нажатия
		await DisplayAlertAsync("Важно",
			"Найдите в открывшемся меню CalendarScrubber и включите переключатель.\nИли найдите пункт 'Автозапуск' в настройках",
			"OK");
		await Request(PermissionType.AutoStart);
	}

	private async Task Request(PermissionType type)
	{
		await _permissionHelper.RequestPermissionAsync(type);
		// Не обновляем статус сразу, так как пользователь ушел в настройки. 
		// Статус обновится в OnAppearing, когда он вернется.
	}

	private async void OnRequestLockScreen(object sender, EventArgs e)
	{
		// Инструкция для пользователя (очень важна, так как настройка спрятана)
		await DisplayAlertAsync("Инструкция для Xiaomi", 
			"1. В открывшемся окне найдите пункт 'Другие разрешения' (Other permissions).\n" +
			"2. Найдите 'Отображать на экране блокировки' (Show on Lock screen).\n" +
			"3. Включите галочку (Разрешить/Allow).", 
			"Понятно, открыть");

		await Request(PermissionType.ShowOnLockScreen);
	}

	private void OnOpenSettingsClicked(object sender, EventArgs e)
	{
		_permissionHelper.OpenAppSettings();
	}

	private async void OnContinueClicked(object sender, EventArgs e)
	{
		// 1. Сохраняем, что пользователь прошел настройку
		Preferences.Set("is_onboarding_completed", true);

		// 2. Закрываем модальное окно
		await Navigation.PopModalAsync();
	}
}