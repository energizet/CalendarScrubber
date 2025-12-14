using CalendarScraber.Models;
using CalendarScraber.Services;

// Не забудь установить NuGet!

namespace CalendarScraber.Pages;

public partial class AlarmPage : ContentPage
{
	private readonly ISystemSoundPlayer _soundPlayer;

	public AlarmPage(CalendarView eventData)
	{
		InitializeComponent();

		_soundPlayer = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<ISystemSoundPlayer>();

		SubjectLabel.Text = eventData.DisplaySubject;
		TimeLabel.Text = $"{eventData.LocalStart:HH:mm} - {eventData.LocalEnd:HH:mm}";

		_soundPlayer.Play();
	}

	private async void OnStopClicked(object sender, EventArgs e)
	{
		_soundPlayer.Stop();
		await Navigation.PopModalAsync();
	}

	// Останавливаем звук, если нажали кнопку "Назад" на телефоне
	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_soundPlayer.Stop();
	}
}