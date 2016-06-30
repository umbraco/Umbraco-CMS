using Umbraco.Core.Events;

namespace Umbraco.Web
{
    internal class HybridEventMessagesAccessor : HybridAccessorBase<EventMessages>, IEventMessagesAccessor
    {
        protected override string HttpContextItemKey => "Umbraco.Core.Events.EventMessages";

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
