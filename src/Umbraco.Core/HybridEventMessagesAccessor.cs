using Umbraco.Core.Cache;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Events;

namespace Umbraco.Web
{
    [UmbracoVolatile]
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
