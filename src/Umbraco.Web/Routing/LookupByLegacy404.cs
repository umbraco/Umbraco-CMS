using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Web.Routing
{
    internal class LookupByLegacy404 : IPublishedContentLookup
    {
        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
        /// </summary>
        /// <param name="pcr">The <c>PublishedContentRequest</c>.</param>		
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public bool TrySetDocument(PublishedContentRequest pcr)
        {
            LogHelper.Debug<LookupByLegacy404>("Looking for a page to handle 404.");

            // TODO - replace the whole logic and stop calling into library!
            var error404 = global::umbraco.library.GetCurrentNotFoundPageId();
            var id = int.Parse(error404);

            IPublishedContent content = null;

            if (id > 0)
            {
                LogHelper.Debug<LookupByLegacy404>("Got id={0}.", () => id);

                content = pcr.RoutingContext.PublishedContentStore.GetDocumentById(
                        pcr.RoutingContext.UmbracoContext,
                        id);

                if (content == null)
                    LogHelper.Debug<LookupByLegacy404>("Could not find content with that id.");
                else
                    LogHelper.Debug<LookupByLegacy404>("Found corresponding content.");
            }
            else
            {
                LogHelper.Debug<LookupByLegacy404>("Got nothing.");
            }

            pcr.PublishedContent = content;
            pcr.Is404 = true;
            return content != null;
        }
    }
}
