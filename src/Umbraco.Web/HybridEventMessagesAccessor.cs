using Umbraco.Core.Events;

namespace Umbraco.Web
{
    internal class HybridEventMessagesAccessor : HybridAccessorBase<EventMessages>, IEventMessagesAccessor
    {
        private const string ItemKeyConst = "Umbraco.Core.Events.HybridEventMessagesAccessor";

        protected override string ItemKey => ItemKeyConst;

        static HybridEventMessagesAccessor()
        {
            SafeCallContextRegister(ItemKeyConst);
        }

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
