using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core
{
    public class HybridEventMessagesAccessor : HybridAccessorBase<EventMessages>, IEventMessagesAccessor
    {
        protected override string ItemKey => "Umbraco.Core.Events.HybridEventMessagesAccessor";

        public HybridEventMessagesAccessor(IRequestCache requestCache)
            : base(requestCache)
        { }

        public EventMessages EventMessages
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}
