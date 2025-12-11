using CalendarScraber.Pages;
using CalendarScraber.Services;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace CalendarScraber;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if ANDROID
		builder.Services.AddSingleton<ICookieExtractor, CookieExtractor>();
#elif WINDOWS
        builder.Services.AddSingleton<ICookieExtractor, CookieExtractor>();
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddTransient<LoginPage>();

		return builder.Build();
	}
}