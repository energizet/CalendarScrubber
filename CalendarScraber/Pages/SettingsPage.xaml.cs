using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarScraber.Services;

namespace CalendarScraber.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
		LoadSettings();
	}

	private void LoadSettings()
	{
		// Загружаем текущие значения в UI
		AlarmSwitch.IsToggled = SettingsManager.IsAlarmEnabled;
		MinutesEntry.Text = SettingsManager.MinutesBefore.ToString();
		ActiveOnlySwitch.IsToggled = SettingsManager.OnlyActiveEvents;
	}

	// Сохраняем при любом изменении
	private void OnSettingsChanged(object sender, EventArgs e)
	{
		SettingsManager.IsAlarmEnabled = AlarmSwitch.IsToggled;
		SettingsManager.OnlyActiveEvents = ActiveOnlySwitch.IsToggled;

		if (int.TryParse(MinutesEntry.Text, out int mins))
		{
			SettingsManager.MinutesBefore = mins;
		}
	}
}