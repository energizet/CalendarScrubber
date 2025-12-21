namespace CalendarScrubber.Services;

public interface IForegroundService
{
	Task Start(string title, string message);
	void Stop();
}