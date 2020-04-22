using Umbraco.Core.Cache;
using Umbraco.Core.Events;

namespace Umbraco.Web
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
