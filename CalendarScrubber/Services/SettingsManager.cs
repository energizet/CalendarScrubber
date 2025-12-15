namespace CalendarScrubber.Services;

public static class SettingsManager
{
	// Ключи для настроек
	private const string KeyAlarmEnabled = "alarm_enabled";
	private const string KeyMinutesBefore = "alarm_minutes_before";
	private const string KeyOnlyActive = "alarm_only_active";
	private const string KeyLastTriggeredId = "alarm_last_id";

	// Свойства
	public static bool IsAlarmEnabled
	{
		get => Preferences.Get(KeyAlarmEnabled, false); // По умолчанию выключен
		set => Preferences.Set(KeyAlarmEnabled, value);
	}

	public static int MinutesBefore
	{
		get => Preferences.Get(KeyMinutesBefore, 15); // По умолчанию 15 минут
		set => Preferences.Set(KeyMinutesBefore, value);
	}

	public static bool OnlyActiveEvents
	{
		get => Preferences.Get(KeyOnlyActive, true); // По умолчанию только активные
		set => Preferences.Set(KeyOnlyActive, value);
	}

	// Запоминаем ID события, о котором уже оповестили, чтобы не орать каждую минуту
	public static string LastTriggeredEventId
	{
		get => Preferences.Get(KeyLastTriggeredId, string.Empty);
		set => Preferences.Set(KeyLastTriggeredId, value);
	}
}