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
        /// <summary>
        /// If there are event messages in the current request this will return them, otherwise it will return null
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <returns></returns>
        public static EventMessages GetCurrentEventMessages(this UmbracoContext umbracoContext)
        {
            // fixme - refactor once we have ServiceScope
            // the event messages factory / accessor should be managed by the container and injected
            // it *probably* should not be 'global' but belong to the service scope if that's not too
            // big of a change, and it would contain messages produced by the scope.
            var scopeContextAdapter = new DefaultScopeContextAdapter();
            var eventMessagesFactory = new ScopeContextEventMessagesFactory(scopeContextAdapter);
            return eventMessagesFactory.GetOrDefault();
        }
    }
}
