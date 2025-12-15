using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CalendarScrubber.Models;

public class LoginRequiredMessage() : ValueChangedMessage<bool>(true);