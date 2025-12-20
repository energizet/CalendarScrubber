using CalendarScrubber.Services;

namespace CalendarScrubber;

public class AppService : IForegroundService
{
	public Task Start(string title, string message)
	{
		// На Windows просто пишем в лог, что служба "запущена"
		System.Diagnostics.Debug.WriteLine($"[Windows Stub] Service Started: {title} - {message}");
		return Task.CompletedTask;
	}

	public void Stop()
	{
		System.Diagnostics.Debug.WriteLine("[Windows Stub] Service Stopped");
	}
}