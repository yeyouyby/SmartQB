using CommunityToolkit.Mvvm.Messaging.Messages;
using SmartQB.Core.Entities;

namespace SmartQB.UI.Messages;

public class AddToBasketMessage : ValueChangedMessage<Question>
{
    public AddToBasketMessage(Question question) : base(question)
    {
    }
}
