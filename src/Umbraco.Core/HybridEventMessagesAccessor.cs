using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core;

/// <summary>
///     A hybrid accessor for <see cref="EventMessages" /> that stores messages in either HTTP context or ambient context.
/// </summary>
public class HybridEventMessagesAccessor : HybridAccessorBase<EventMessages>, IEventMessagesAccessor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HybridEventMessagesAccessor" /> class.
    /// </summary>
    /// <param name="requestCache">The request cache for storing the value in the HTTP context.</param>
    public HybridEventMessagesAccessor(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    /// <inheritdoc />
    public EventMessages? EventMessages
    {
        get => Value;
        set => Value = value;
    }
}
