using Umbraco.Core.Events;

namespace Umbraco.Web
{
    internal class HybridEventMessagesAccessor : HybridAccessorBase<EventMessages>, IEventMessagesAccessor
    {
        protected override string ItemKey => "Umbraco.Core.Events.HybridEventMessagesAccessor";

        public HybridEventMessagesAccessor(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        { }

        public EventMessages EventMessages
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}
