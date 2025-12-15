using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CalendarScrubber.Models;

public class UpdateMessage() : ValueChangedMessage<bool>(true);