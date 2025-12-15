using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CalendarScraber.Models;

public class UpdateMessage() : ValueChangedMessage<bool>(true);