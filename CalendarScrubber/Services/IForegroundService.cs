namespace CalendarScrubber.Services;

public interface IForegroundService
{
	void Start(string title, string message);
	void Stop();
}