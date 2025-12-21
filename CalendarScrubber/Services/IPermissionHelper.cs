namespace CalendarScrubber.Services;

public enum PermissionType
{
	Notifications,      // Уведомления (Android 13+)
	ExactAlarms,        // Точные будильники (Android 12+)
	Overlay,            // Поверх окон (для кастомного UI)
	FullScreenIntent,   // Разворачивать на весь экран (Android 14+)
	BatteryOptimization, // Игнорировать оптимизацию (чтобы не убили службу)
	AutoStart,
	ShowOnLockScreen
}

public interface IPermissionHelper
{
	// Возвращает true, если разрешение выдано
	Task<bool> CheckStatusAsync(PermissionType type);
    
	// Пытается запросить разрешение или открывает настройки
	Task RequestPermissionAsync(PermissionType type);
    
	// Открыть страницу "О приложении" (универсальная кнопка)
	void OpenAppSettings();
}