using System;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// Stores the instance of EventMessages in the current scope.
    /// </summary>
    internal class ScopeLifespanMessagesFactory : IEventMessagesFactory
    {
        public const string ContextKey = "Umbraco.Core.Events.ScopeLifespanMessagesFactory";

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IScopeProviderInternal _scopeProvider;

        public static ScopeLifespanMessagesFactory Current { get; private set; }

        public ScopeLifespanMessagesFactory(IHttpContextAccessor contextAccesor, IScopeProvider scopeProvider)
        {
            if (contextAccesor == null) throw new ArgumentNullException("contextAccesor");
            if (scopeProvider == null) throw new ArgumentNullException("scopeProvider");
            if (scopeProvider is IScopeProviderInternal == false) throw new ArgumentException("Not IScopeProviderInternal.", "scopeProvider");
            _contextAccessor = contextAccesor;
            _scopeProvider = (IScopeProviderInternal) scopeProvider;
            Current = this;
        }

        public EventMessages Get()
        {
            var messages = GetFromHttpContext();
            if (messages != null) return messages;

            var scope = _scopeProvider.GetAmbientOrNoScope();
            return scope.Messages;
        }

        public EventMessages GetFromHttpContext()
        {
            if (_contextAccessor == null || _contextAccessor.Value == null) return null;
            return (EventMessages)_contextAccessor.Value.Items[ContextKey];
        }

        public EventMessages TryGet()
        {
            var messages = GetFromHttpContext();
            if (messages != null) return messages;

            var scope = _scopeProvider.AmbientScope;
            return scope == null ? null : scope.MessagesOrNull;
        }

        public void Set(EventMessages messages)
        {
            if (_contextAccessor.Value == null) return;
            _contextAccessor.Value.Items[ContextKey] = messages;
        }
    }
}
