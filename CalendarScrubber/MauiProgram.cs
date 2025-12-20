using CalendarScrubber.Pages;
using CalendarScrubber.Services;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace CalendarScrubber;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if ANDROID
			.UseLocalNotification()
#endif
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton(Plugin.Maui.Audio.AudioManager.Current);

#if ANDROID
		builder.Services.AddSingleton<ICookieExtractor, CookieExtractor>();
		builder.Services.AddSingleton<ISystemAlarmService, SystemAlarmService>();
		builder.Services.AddSingleton<IForegroundService, AppService>();
		builder.Services.AddSingleton<ISystemSoundPlayer, SystemSoundPlayer>();
#elif WINDOWS
        builder.Services.AddSingleton<ICookieExtractor, CookieExtractor>();
        builder.Services.AddSingleton<ISystemAlarmService, SystemAlarmService>();
        builder.Services.AddSingleton<IForegroundService, AppService>();
        builder.Services.AddSingleton<ISystemSoundPlayer, SystemSoundPlayer>();
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddSingleton<CalendarService>();
		builder.Services.AddSingleton<AlarmService>();
		builder.Services.AddSingleton<IEventStorage, EventStorage>();

		return builder.Build();
	}
}