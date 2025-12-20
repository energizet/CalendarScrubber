namespace CalendarScrubber;

public partial class App : Application
{
	private readonly IServiceProvider _serviceProvider;

	public App(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		InitializeComponent();
		CheckFirstRun();
	}

	private void CheckFirstRun()
	{
		// Проверяем, есть ли флаг завершения настройки
		var isCompleted = Preferences.Get("is_onboarding_completed", false);

		if (!isCompleted)
		{
			// Используем Dispatcher, чтобы убедиться, что Shell уже загрузился
			// и готов к навигации
			Dispatcher.Dispatch(async () =>
			{
				// Ждем небольшую паузу, чтобы UI успел отрисоваться (защита от краша на старте)
				await Task.Delay(100);

				if (Shell.Current != null)
				{
					// Передаем true, чтобы включить режим онбординга (кнопку "Продолжить")
					await Shell.Current.Navigation.PushModalAsync(new Pages.PermissionsPage(_serviceProvider,
						isOnboarding: true));
				}
			});
		}
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new(new AppShell());
	}
}