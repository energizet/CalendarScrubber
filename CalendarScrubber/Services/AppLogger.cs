using CommunityToolkit.Mvvm.Messaging;
using CalendarScrubber.Models;

namespace CalendarScrubber.Services;

public static class AppLogger
{
	public static void Log(string message)
	{
		// Пишем и в консоль отладки, и в UI
		System.Diagnostics.Debug.WriteLine($"[APP LOG] {DateTime.Now} {message}");
        
		var logEntry = new AppLog(message);
		WeakReferenceMessenger.Default.Send(new LogTriggeredMessage(logEntry));
	}
}