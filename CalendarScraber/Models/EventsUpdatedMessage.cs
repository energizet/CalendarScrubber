using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CalendarScraber.Models;

public class EventsUpdatedMessage(List<CalendarView> events) : ValueChangedMessage<List<CalendarView>>(events);