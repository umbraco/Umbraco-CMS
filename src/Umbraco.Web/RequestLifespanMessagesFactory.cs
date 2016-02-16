using System;
using Umbraco.Core.Events;

namespace Umbraco.Web
{
    /// <summary>
    /// Stores the instance of EventMessages in the current request so all events will share the same instance
    /// </summary>
    internal class RequestLifespanMessagesFactory : IEventMessagesFactory
    {
        private readonly IHttpContextAccessor _httpAccessor;

        public RequestLifespanMessagesFactory(IHttpContextAccessor httpAccessor)
        {
            if (httpAccessor == null) throw new ArgumentNullException("httpAccessor");
            _httpAccessor = httpAccessor;
        }

        public EventMessages Get()
        {
            if (_httpAccessor.Value.Items[typeof (RequestLifespanMessagesFactory).Name] == null)
            {
                _httpAccessor.Value.Items[typeof(RequestLifespanMessagesFactory).Name] = new EventMessages();
            }
            return (EventMessages)_httpAccessor.Value.Items[typeof (RequestLifespanMessagesFactory).Name];
        }
    }
}