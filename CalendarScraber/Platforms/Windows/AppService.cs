using CalendarScraber.Services;

namespace CalendarScraber;

public class AppService : IForegroundService
{
	public void Start(string title, string message)
	{
		// На Windows просто пишем в лог, что служба "запущена"
		System.Diagnostics.Debug.WriteLine($"[Windows Stub] Service Started: {title} - {message}");
	}

	public void Stop()
	{
		System.Diagnostics.Debug.WriteLine("[Windows Stub] Service Stopped");
	}
}