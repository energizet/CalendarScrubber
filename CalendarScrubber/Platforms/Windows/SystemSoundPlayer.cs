using CalendarScrubber.Services;

namespace CalendarScrubber;

public class SystemSoundPlayer : ISystemSoundPlayer
{
	public void Play()
	{
		// На Windows просто бипнем системным динамиком
		System.Console.Beep(1000, 2000); 
	}

	public void Stop()
	{
		// Нечего останавливать
	}
}