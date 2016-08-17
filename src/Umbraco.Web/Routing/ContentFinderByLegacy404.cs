using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using Umbraco.Core;
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

            // try to find a culture as best as we can
		    var errorCulture = CultureInfo.CurrentUICulture;
		    if (pcr.HasDomain)
		    {
		        errorCulture = CultureInfo.GetCultureInfo(pcr.UmbracoDomain.LanguageIsoCode);
		    }
		    else
		    {
		        var route = pcr.Uri.GetAbsolutePathDecoded();
		        var pos = route.LastIndexOf('/');
		        IPublishedContent node = null;
		        while (pos > 1)
		        {
		            route = route.Substring(0, pos);
                    node = pcr.RoutingContext.UmbracoContext.ContentCache.GetByRoute(route);
		            if (node != null) break;
                    pos = route.LastIndexOf('/');
                }
		        if (node != null)
		        {
		            var d = DomainHelper.FindWildcardDomainInPath(pcr.RoutingContext.UmbracoContext.Application.Services.DomainService.GetAll(true), node.Path, null);
		            if (d != null && string.IsNullOrWhiteSpace(d.LanguageIsoCode) == false)
		                errorCulture = CultureInfo.GetCultureInfo(d.LanguageIsoCode);
		        }
            }

		    var error404 = NotFoundHandlerHelper.GetCurrentNotFoundPageId(
                _contentConfigSection.Error404Collection.ToArray(),
                pcr.RoutingContext.UmbracoContext.Application.Services.EntityService,
                new PublishedContentQuery(pcr.RoutingContext.UmbracoContext.ContentCache, pcr.RoutingContext.UmbracoContext.MediaCache),
                errorCulture);

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
