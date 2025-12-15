using CalendarScrubber.Services;

namespace CalendarScrubber.Pages;

public partial class SettingsPage : ContentPage
{
	private bool _isLoading = false;

	public SettingsPage()
	{
		InitializeComponent();
		LoadSettings();
	}

	private void LoadSettings()
	{
		_isLoading = true;

		try
		{
			// Загружаем текущие значения в UI
			AlarmSwitch.IsToggled = SettingsManager.IsAlarmEnabled;
			MinutesEntry.Text = SettingsManager.MinutesBefore.ToString();
			ActiveOnlySwitch.IsToggled = SettingsManager.OnlyActiveEvents;
		}
		finally
		{
			// 2. Опускаем флаг, когда всё установили
			_isLoading = false;
		}
	}

	// Сохраняем при любом изменении
	private void OnSettingsChanged(object sender, EventArgs e)
	{
		if (_isLoading) return;

		SettingsManager.IsAlarmEnabled = AlarmSwitch.IsToggled;
		SettingsManager.OnlyActiveEvents = ActiveOnlySwitch.IsToggled;

		if (int.TryParse(MinutesEntry.Text, out var mins))
		{
			SettingsManager.MinutesBefore = mins;
		}
	}
}