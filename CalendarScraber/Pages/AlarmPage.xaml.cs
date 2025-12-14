using CalendarScraber.Models;
using CalendarScraber.Services;

// Не забудь установить NuGet!

namespace CalendarScraber.Pages;

public partial class AlarmPage : ContentPage
{
	private readonly CalendarView _eventData;
	private readonly ISystemSoundPlayer? _soundPlayer;
	private readonly ISystemAlarmService? _alarmService;

	public AlarmPage(CalendarView eventData)
	{
		_eventData = eventData;
		InitializeComponent();
		
		AppLogger.Log($"Открыто окно тревоги: {eventData.Subject}");
		
		_soundPlayer = Application.Current?.Handler?.MauiContext?.Services.GetService<ISystemSoundPlayer>();
		_alarmService = Application.Current?.Handler?.MauiContext?.Services.GetService<ISystemAlarmService>();

		SubjectLabel.Text = eventData.DisplaySubject;
		TimeLabel.Text = $"{eventData.LocalStart:HH:mm} - {eventData.LocalEnd:HH:mm}";
	}

	private async void OnStopClicked(object sender, EventArgs e)
	{
		AppLogger.Log("🔕 Нажата кнопка стоп в приложении");
		_soundPlayer?.Stop();
		_alarmService?.CancelNotification(_eventData.ItemId.Id);
		await Navigation.PopModalAsync();
	}

	// Останавливаем звук, если нажали кнопку "Назад" на телефоне
	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		
		AppLogger.Log("🔕 Нажата кнопка Назад на телефоне");
		_soundPlayer?.Stop();
		_alarmService?.CancelNotification(_eventData.ItemId.Id);
	}
}