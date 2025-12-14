using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CalendarScraber.Models;

public class AppLog(string message)
{
	public DateTime Timestamp { get; } = DateTime.Now;
	public string Message { get; set; } = message;

	public string DisplayTime => Timestamp.ToString("HH:mm:ss");
}

public class LogTriggeredMessage(AppLog message) : ValueChangedMessage<AppLog>(message);