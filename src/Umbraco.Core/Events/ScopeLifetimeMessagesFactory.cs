// fixme - remove this file
//using System;
//using Umbraco.Core.Scoping;

//namespace Umbraco.Core.Events
//{
//    /// <summary>
//    /// Stores the instance of EventMessages in the current scope.
//    /// </summary>
//    internal class ScopeLifetimeMessagesFactory : IEventMessagesFactory
//    {
//        public const string ContextKey = "Umbraco.Core.Events.ScopeLifetimeMessagesFactory";

//        // fixme for v8 that one will need to be entirely and massively refactored

//        private readonly IHttpContextAccessor _contextAccessor;
//        private readonly IScopeProviderInternal _scopeProvider;

//        public static ScopeLifetimeMessagesFactory Current { get; private set; }

//        public ScopeLifetimeMessagesFactory(IHttpContextAccessor contextAccesor, IScopeProvider scopeProvider)
//        {
//            if (scopeProvider == null) throw new ArgumentNullException(nameof(scopeProvider));
//            if (scopeProvider is IScopeProviderInternal == false) throw new ArgumentException("Not IScopeProviderInternal.", nameof(scopeProvider));
//            _contextAccessor = contextAccesor ?? throw new ArgumentNullException(nameof(contextAccesor));
//            _scopeProvider = (IScopeProviderInternal) scopeProvider;
//            Current = this;
//        }

//        public EventMessages Get()
//        {
//            var messages = GetFromHttpContext();
//            if (messages != null) return messages;

//            var scope = _scopeProvider.GetAmbientOrNoScope();
//            return scope.Messages;
//        }

//        public EventMessages GetFromHttpContext()
//        {
//            if (_contextAccessor == null || _contextAccessor.Value == null) return null;
//            return (EventMessages)_contextAccessor.Value.Items[ContextKey];
//        }

//        public EventMessages TryGet()
//        {
//            var messages = GetFromHttpContext();
//            if (messages != null) return messages;

//            var scope = _scopeProvider.AmbientScope;
//            return scope?.MessagesOrNull;
//        }

//        public void Set(EventMessages messages)
//        {
//            if (_contextAccessor.Value == null) return;
//            _contextAccessor.Value.Items[ContextKey] = messages;
//        }
//    }
//}
