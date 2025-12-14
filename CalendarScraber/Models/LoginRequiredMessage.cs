using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CalendarScraber.Models;

public class LoginRequiredMessage() : ValueChangedMessage<bool>(true);