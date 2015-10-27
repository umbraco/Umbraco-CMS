using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Events;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods for <see cref="UmbracoContext"/>.
    /// </summary>
    public static class UmbracoContextExtensions
    {

        /// <summary>
        /// If there are event messages in the current request this will return them , otherwise it will return null
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <returns></returns>
        public static EventMessages GetCurrentEventMessages(this UmbracoContext umbracoContext)
        {
            var msgs = umbracoContext.HttpContext.Items[typeof (RequestLifespanMessagesFactory).Name];
            if (msgs == null) return null;
            return (EventMessages) msgs;
        }

    }
}
