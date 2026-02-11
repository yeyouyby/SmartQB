using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SmartQB.UI.Messages;

public class PrintHtmlMessage : ValueChangedMessage<string>
{
    public PrintHtmlMessage(string html) : base(html)
    {
    }
}
