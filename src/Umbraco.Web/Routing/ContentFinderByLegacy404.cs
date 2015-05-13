using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;

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
                new PublishedContentQuery(pcr.RoutingContext.UmbracoContext.ContentCache, pcr.RoutingContext.UmbracoContext.MediaCache));

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
    
    internal class NotFoundHandlerHelper
    {
        /// <summary>
        /// Returns the Umbraco page id to use as the Not Found page based on the configured 404 pages and the current request
        /// </summary>
        /// <param name="error404Collection"></param>
        /// <param name="requestServerName">
        /// The server name attached to the request, normally would be the source of HttpContext.Current.Request.ServerVariables["SERVER_NAME"]
        /// </param>
        /// <param name="entityService"></param>
        /// <param name="publishedContentQuery"></param>
        /// <returns></returns>
        internal static int? GetCurrentNotFoundPageId(
            IContentErrorPage[] error404Collection,
            string requestServerName,
            IEntityService entityService,
            PublishedContentQuery publishedContentQuery)
        {
            if (error404Collection.Count() > 1)
            {
                // try to get the 404 based on current culture (via domain)
                IContentErrorPage cultureErr;

                //TODO: Remove the dependency on this legacy Domain service, 
                // in 7.3 the real domain service should be passed in as a parameter.
                if (Domain.Exists(requestServerName))
                {
                    var d = Domain.GetDomain(requestServerName);

                    // test if a 404 page exists with current culture
                    cultureErr = error404Collection
                        .FirstOrDefault(x => x.Culture == d.Language.CultureAlias);

                    if (cultureErr != null)
                    {
                        return GetContentIdFromErrorPageConfig(cultureErr, entityService, publishedContentQuery);
                    }
                }

                // test if a 404 page exists with current culture thread
                cultureErr = error404Collection
                    .FirstOrDefault(x => x.Culture == System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                if (cultureErr != null)
                {
                    return GetContentIdFromErrorPageConfig(cultureErr, entityService, publishedContentQuery);
                }

                // there should be a default one!
                cultureErr = error404Collection
                    .FirstOrDefault(x => x.Culture == "default");

                if (cultureErr != null)
                {
                    return GetContentIdFromErrorPageConfig(cultureErr, entityService, publishedContentQuery);
                }
            }
            else
            {
                return GetContentIdFromErrorPageConfig(error404Collection.First(), entityService, publishedContentQuery);
            }

            return null;
        }

        /// <summary>
        /// Returns the content id based on the configured IContentErrorPage section
        /// </summary>
        /// <param name="errorPage"></param>
        /// <param name="entityService"></param>
        /// <param name="publishedContentQuery"></param>
        /// <returns></returns>
        internal static int? GetContentIdFromErrorPageConfig(IContentErrorPage errorPage, IEntityService entityService, PublishedContentQuery publishedContentQuery)
        {
            if (errorPage.HasContentId) return errorPage.ContentId;

            if (errorPage.HasContentKey)
            {
                //need to get the Id for the GUID
                //TODO: When we start storing GUIDs into the IPublishedContent, then we won't have to look this up 
                // but until then we need to look it up in the db. For now we've implemented a cached service for 
                // converting Int -> Guid and vice versa.
                var found = entityService.GetIdForKey(errorPage.ContentKey, UmbracoObjectTypes.Document);
                if (found)
                {
                    return found.Result;
                }
                return null;
            }

            if (errorPage.ContentXPath.IsNullOrWhiteSpace() == false)
            {
                try
                {
                    //we have an xpath statement to execute
                    var xpathResult = UmbracoXPathPathSyntaxParser.ParseXPathQuery(
                        xpathExpression: errorPage.ContentXPath,
                        nodeContextId: null,
                        getPath: nodeid =>
                        {
                            var ent = entityService.Get(nodeid);
                            return ent.Path.Split(',').Reverse();
                        },
                        publishedContentExists: i => publishedContentQuery.TypedContent(i) != null);

                    //now we'll try to execute the expression
                    var nodeResult = publishedContentQuery.TypedContentSingleAtXPath(xpathResult);
                    if (nodeResult != null)
                        return nodeResult.Id;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<NotFoundHandlerHelper>("Could not parse xpath expression: " + errorPage.ContentXPath, ex);
                    return null;
                }
            }
            return null;
        }

    }
}
