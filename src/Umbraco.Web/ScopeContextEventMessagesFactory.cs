using System;
using Umbraco.Core;
using Umbraco.Core.Events;

namespace Umbraco.Web
{
    /// <summary>
    /// Stores the instance of EventMessages in the current scope context so all events will share the same instance.
    /// </summary>
    internal class ScopeContextEventMessagesFactory : IEventMessagesFactory
    {
        private readonly IScopeContextAdapter _scopeContextAdapter;
        private const string ContextKey = nameof(ScopeContextEventMessagesFactory);

        public ScopeContextEventMessagesFactory(IScopeContextAdapter scopeContextAdapter)
        {
            if (scopeContextAdapter == null) throw new ArgumentNullException(nameof(scopeContextAdapter));
            _scopeContextAdapter = scopeContextAdapter;
        }

        public EventMessages Get()
        {
            var evtMsgs = (EventMessages) _scopeContextAdapter.Get(ContextKey);
            if (evtMsgs == null)
                _scopeContextAdapter.Set(ContextKey, evtMsgs = new EventMessages());
            return evtMsgs;
        }

        public EventMessages GetOrDefault()
        {
            return (EventMessages) _scopeContextAdapter.Get(ContextKey);
        }
    }
}