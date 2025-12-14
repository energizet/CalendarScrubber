using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CalendarScraber.Models;

public class AlarmTriggeredMessage(CalendarView eventData) : ValueChangedMessage<CalendarView>(eventData);