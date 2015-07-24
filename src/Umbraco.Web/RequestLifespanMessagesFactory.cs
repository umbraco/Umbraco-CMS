using System;
using Umbraco.Core.Events;

namespace Umbraco.Web
{
    /// <summary>
    /// Stores the instance of EventMessages in the current request so all events will share the same instance
    /// </summary>
    internal class RequestLifespanMessagesFactory : IEventMessagesFactory
    {
        private readonly IUmbracoContextAccessor _ctxAccessor;

        public RequestLifespanMessagesFactory(IUmbracoContextAccessor ctxAccessor)
        {
            if (ctxAccessor == null) throw new ArgumentNullException("ctxAccessor");
            _ctxAccessor = ctxAccessor;
        }

        public EventMessages Get()
        {
            if (_ctxAccessor.Value.HttpContext.Items[typeof (RequestLifespanMessagesFactory).Name] == null)
            {
                _ctxAccessor.Value.HttpContext.Items[typeof(RequestLifespanMessagesFactory).Name] = new EventMessages();
            }
            return (EventMessages)_ctxAccessor.Value.HttpContext.Items[typeof (RequestLifespanMessagesFactory).Name];
        }
    }
}