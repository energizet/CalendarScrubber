using CommunityToolkit.Mvvm.Messaging;
using CalendarScraber.Models;

namespace CalendarScraber.Services;

public static class AppLogger
{
	public static void Log(string message)
	{
		// Пишем и в консоль отладки, и в UI
		System.Diagnostics.Debug.WriteLine($"[APP LOG] {message}");
        
		var logEntry = new AppLog(message);
		WeakReferenceMessenger.Default.Send(new LogTriggeredMessage(logEntry));
	}
}