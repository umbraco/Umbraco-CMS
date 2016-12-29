using System;
using System.Runtime.Remoting.Messaging;
using Umbraco.Core.Events;

namespace Umbraco.Web
{
    /// <summary>
    /// Stores the instance of EventMessages in the current request so all events will share the same instance
    /// </summary>
    internal class RequestLifespanMessagesFactory : IEventMessagesFactory
    {
        private const string ContextKey = "Umbraco.Web.RequestLifespanMessagesFactory";
        private readonly IHttpContextAccessor _httpAccessor;

        public RequestLifespanMessagesFactory(IHttpContextAccessor httpAccessor)
        {
            if (httpAccessor == null) throw new ArgumentNullException("httpAccessor");
            _httpAccessor = httpAccessor;
        }

        public EventMessages Get()
        {
            var httpContext = _httpAccessor.Value;
            if (httpContext != null)
            {
                var eventMessages = httpContext.Items[ContextKey] as EventMessages;
                if (eventMessages == null) httpContext.Items[ContextKey] = eventMessages = new EventMessages();
                return eventMessages;
            }

            var lccContext = CallContext.LogicalGetData(ContextKey) as EventMessages;
            if (lccContext != null) return lccContext;

            throw new Exception("Could not get messages.");
        }

        public EventMessages TryGet()
        {
            var httpContext = _httpAccessor.Value;
            return httpContext != null
                ? httpContext.Items[ContextKey] as EventMessages
                : CallContext.LogicalGetData(ContextKey) as EventMessages;
        }

        // Deploy wants to execute things outside of a request, where this factory would fail,
        // so the factory is extended so that Deploy can Set/Clear event messages in the logical
        // call context (which flows with async) - it needs to be set and cleared because, contrary
        // to http context, it's not being cleared at the end of anything.
        //
        // to be refactored in v8! the whole IEventMessagesFactory is borked anyways

        public void SetLlc()
        {
            CallContext.LogicalSetData(ContextKey, new EventMessages());
        }

        public void ClearLlc()
        {
            CallContext.FreeNamedDataSlot(ContextKey);
        }
    }
}