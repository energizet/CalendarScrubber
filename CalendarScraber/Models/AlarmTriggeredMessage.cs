using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CalendarScraber.Models;

public class AlarmTriggeredMessage : ValueChangedMessage<CalendarView>
{
	public AlarmTriggeredMessage(CalendarView eventData) : base(eventData)
	{
	}
}