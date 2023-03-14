namespace Umbraco.Cms.Core.Events;

public interface IEventDefinition
{
    object Sender { get; }

    object Args { get; }

    string? EventName { get; }

    void RaiseEvent();
}
