using System.Linq;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that runs the legacy 404 logic.
	/// </summary>
	public class ContentFinderByLegacy404 : IContentFinder
	{
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="pcr">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TryFindContent(PublishedContentRequest pcr)
		{
			LogHelper.Debug<ContentFinderByLegacy404>("Looking for a page to handle 404.");

            // TODO - replace the whole logic
		    var error404 = NotFoundHandlerHelper.GetCurrentNotFoundPageId(
                //TODO: The IContentSection should be ctor injected into this class in v8!
		        UmbracoConfig.For.UmbracoSettings().Content.Error404Collection.ToArray(),
                //TODO: Is there a better way to extract this value? at least we're not relying on singletons here though
		        pcr.RoutingContext.UmbracoContext.HttpContext.Request.ServerVariables["SERVER_NAME"],
                pcr.RoutingContext.UmbracoContext.Application.Services.EntityService,
                new PublishedContentQuery(pcr.RoutingContext.UmbracoContext.ContentCache, pcr.RoutingContext.UmbracoContext.MediaCache),
                pcr.RoutingContext.UmbracoContext.Application.Services.DomainService);

			IPublishedContent content = null;

            if (error404.HasValue)
			{
                LogHelper.Debug<ContentFinderByLegacy404>("Got id={0}.", () => error404.Value);

                content = pcr.RoutingContext.UmbracoContext.ContentCache.GetById(error404.Value);

			    LogHelper.Debug<ContentFinderByLegacy404>(content == null
			        ? "Could not find content with that id."
			        : "Found corresponding content.");
			}
			else
			{
				LogHelper.Debug<ContentFinderByLegacy404>("Got nothing.");
			}

			pcr.PublishedContent = content;
            pcr.SetIs404();
			return content != null;
		}
	}
}
