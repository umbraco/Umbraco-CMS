using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core;

public class HybridEventMessagesAccessor : HybridAccessorBase<EventMessages>, IEventMessagesAccessor
{
    public HybridEventMessagesAccessor(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public EventMessages? EventMessages
    {
        get => Value;
        set => Value = value;
    }
}
