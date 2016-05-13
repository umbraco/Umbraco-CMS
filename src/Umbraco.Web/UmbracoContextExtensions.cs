using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Events;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods for <see cref="UmbracoContext"/>.
    /// </summary>
    public static class UmbracoContextExtensions
    {
        // fixme - this class is generally ugly now that we have proper IoC
        // the current umbraco context belongs to the ScopeContext not the HttpContext

        /// <summary>
        /// tries to get the Umbraco context from the HttpContext
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is useful when working on async threads since the UmbracoContext is not copied over explicitly
        /// </remarks>
        public static UmbracoContext GetUmbracoContext(this HttpContext http)
        {
            return GetUmbracoContext(new HttpContextWrapper(http));
        }

        /// <summary>
        /// tries to get the Umbraco context from the HttpContext
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is useful when working on async threads since the UmbracoContext is not copied over explicitly
        /// </remarks>
        public static UmbracoContext GetUmbracoContext(this HttpContextBase http)
        {
            if (http == null) throw new ArgumentNullException("http");

            if (http.Items.Contains(UmbracoContext.HttpContextItemName))
            {
                var umbCtx = http.Items[UmbracoContext.HttpContextItemName] as UmbracoContext;
                return umbCtx;
            }
            return null;
        }

        /// <summary>
        /// If there are event messages in the current request this will return them , otherwise it will return null
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <returns></returns>
        public static EventMessages GetCurrentEventMessages(this UmbracoContext umbracoContext)
        {
            // fixme - this is ugly
            // the event messages factory should be injected / supplied by the container to whoever needs it!
            var scopeContextAdapter = new DefaultScopeContextAdapter();
            var eventMessagesFactory = new ScopeContextEventMessagesFactory(scopeContextAdapter);
            return eventMessagesFactory.GetOrDefault();
        }
    }
}
