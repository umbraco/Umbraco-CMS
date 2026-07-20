using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core;

/// <summary>
///     Default implementation of <see cref="IEventMessagesFactory" /> that creates and manages event messages.
/// </summary>
public class DefaultEventMessagesFactory : IEventMessagesFactory
{
    private readonly IEventMessagesAccessor _eventMessagesAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultEventMessagesFactory" /> class.
    /// </summary>
    /// <param name="eventMessagesAccessor">The accessor for getting and setting event messages.</param>
    /// <exception cref="ArgumentNullException">The event messages accessor is null.</exception>
    public DefaultEventMessagesFactory(IEventMessagesAccessor eventMessagesAccessor)
    {
        _eventMessagesAccessor = eventMessagesAccessor ?? throw new ArgumentNullException(nameof(eventMessagesAccessor));
    }

    /// <inheritdoc />
    public EventMessages Get()
    {
        EventMessages? eventMessages = _eventMessagesAccessor.EventMessages;
        if (eventMessages == null)
        {
            _eventMessagesAccessor.EventMessages = eventMessages = new EventMessages();
        }

        return eventMessages;
    }

    /// <inheritdoc />
    public EventMessages? GetOrDefault() => _eventMessagesAccessor.EventMessages;
}
