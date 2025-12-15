using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CalendarScrubber.Models;

public class EventsUpdatedMessage(List<CalendarView> events) : ValueChangedMessage<List<CalendarView>>(events);