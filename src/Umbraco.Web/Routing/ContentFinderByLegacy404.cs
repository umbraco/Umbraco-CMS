using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that runs the legacy 404 logic.
	/// </summary>
	public class ContentFinderByLegacy404 : IContentFinder
	{
	    
        private readonly ILogger _logger;
	    private readonly IContentSection _contentConfigSection;

	    public ContentFinderByLegacy404(ILogger logger, IContentSection contentConfigSection)
	    {
	        _logger = logger;
	        _contentConfigSection = contentConfigSection;
	    }

	    /// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="pcr">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TryFindContent(PublishedContentRequest pcr)
		{
			_logger.Debug<ContentFinderByLegacy404>("Looking for a page to handle 404.");

		    var error404 = NotFoundHandlerHelper.GetCurrentNotFoundPageId(
                _contentConfigSection.Error404Collection.ToArray(),
                //TODO: Is there a better way to extract this value? at least we're not relying on singletons here though
		        pcr.RoutingContext.UmbracoContext.HttpContext.Request.ServerVariables["SERVER_NAME"],
                pcr.RoutingContext.UmbracoContext.Application.Services.EntityService,
                new PublishedContentQuery(pcr.RoutingContext.UmbracoContext.ContentCache, pcr.RoutingContext.UmbracoContext.MediaCache),
                pcr.RoutingContext.UmbracoContext.Application.Services.DomainService);

			IPublishedContent content = null;

            if (error404.HasValue)
			{
                _logger.Debug<ContentFinderByLegacy404>("Got id={0}.", () => error404.Value);

                content = pcr.RoutingContext.UmbracoContext.ContentCache.GetById(error404.Value);

			    _logger.Debug<ContentFinderByLegacy404>(content == null
			        ? "Could not find content with that id."
			        : "Found corresponding content.");
			}
			else
			{
				_logger.Debug<ContentFinderByLegacy404>("Got nothing.");
			}

			pcr.PublishedContent = content;
            pcr.SetIs404();
			return content != null;
		}
	}
}
