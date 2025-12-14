using CommunityToolkit.Mvvm.Messaging; // <-- Добавить
using CalendarScraber.Models; // <-- Добавить

namespace CalendarScraber;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
       
		// ПОДПИСКА НА БУДИЛЬНИК
		WeakReferenceMessenger.Default.Register<AlarmTriggeredMessage>(this, (r, m) =>
		{
			// m.Value - это уже CalendarView
			var eventObj = m.Value;

			MainThread.BeginInvokeOnMainThread(async () =>
			{
				if (Shell.Current != null)
				{
					// Передаем объект прямо в страницу
					await Shell.Current.Navigation.PushModalAsync(new Pages.AlarmPage(eventObj));
				}
			});
		});
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new(new AppShell());
	}
}