using System;
using Umbraco.Core.Events;

namespace Umbraco.Web
{
    internal class DefaultEventMessagesFactory : IEventMessagesFactory
    {
        private readonly IEventMessagesAccessor _eventMessagesAccessor;

        public DefaultEventMessagesFactory(IEventMessagesAccessor eventMessagesAccessor)
        {
            if (eventMessagesAccessor == null) throw new ArgumentNullException(nameof(eventMessagesAccessor));
            _eventMessagesAccessor = eventMessagesAccessor;
        }

        public EventMessages Get()
        {
            var eventMessages = _eventMessagesAccessor.EventMessages;
            if (eventMessages == null)
                _eventMessagesAccessor.EventMessages = eventMessages = new EventMessages();
            return eventMessages;
        }

        public EventMessages GetOrDefault()
        {
            return _eventMessagesAccessor.EventMessages;
        }
    }
}
