using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core;

public class DefaultEventMessagesFactory : IEventMessagesFactory
{
    private readonly IEventMessagesAccessor _eventMessagesAccessor;

    public DefaultEventMessagesFactory(IEventMessagesAccessor eventMessagesAccessor)
    {
        _eventMessagesAccessor = eventMessagesAccessor ?? throw new ArgumentNullException(nameof(eventMessagesAccessor));
    }

    public EventMessages Get()
    {
        EventMessages? eventMessages = _eventMessagesAccessor.EventMessages;
        if (eventMessages == null)
        {
            _eventMessagesAccessor.EventMessages = eventMessages = new EventMessages();
        }

        return eventMessages;
    }

    public EventMessages? GetOrDefault() => _eventMessagesAccessor.EventMessages;
}
