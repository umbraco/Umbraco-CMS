using System;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Used to determine the node to display when content is not found based on the configured error404 elements in umbracoSettings.config
    /// </summary>
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
        /// <param name="domainService"></param>
        /// <returns></returns>
        internal static int? GetCurrentNotFoundPageId(
            IContentErrorPage[] error404Collection,
            string requestServerName,
            IEntityService entityService,
            IPublishedContentQuery publishedContentQuery,
            IDomainService domainService)
        {
            throw new NotImplementedException();
        }

        internal static int? GetCurrentNotFoundPageId(
            IContentErrorPage[] error404Collection,
            IEntityService entityService,
            IPublishedContentQuery publishedContentQuery,
            CultureInfo errorCulture,
            int? domainContentId)
        {
            if (error404Collection.Length > 1)
            {
                // test if a 404 page exists with current culture thread
                var cultureErr = error404Collection.FirstOrDefault(x => x.Culture == errorCulture.Name)
                    ?? error404Collection.FirstOrDefault(x => x.Culture == "default"); // there should be a default one!

                if (cultureErr != null)
                    return GetContentIdFromErrorPageConfig(cultureErr, entityService, publishedContentQuery, domainContentId);
            }
            else
            {
                return GetContentIdFromErrorPageConfig(error404Collection.First(), entityService, publishedContentQuery, domainContentId);
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
        internal static int? GetContentIdFromErrorPageConfig(IContentErrorPage errorPage, IEntityService entityService, IPublishedContentQuery publishedContentQuery, int? domainContentId)
        {
            if (errorPage.HasContentId) return errorPage.ContentId;

            if (errorPage.HasContentKey)
            {
                //need to get the Id for the GUID
                // TODO: When we start storing GUIDs into the IPublishedContent, then we won't have to look this up
                // but until then we need to look it up in the db. For now we've implemented a cached service for
                // converting Int -> Guid and vice versa.
                var found = entityService.GetId(errorPage.ContentKey, UmbracoObjectTypes.Document);
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
                        nodeContextId: domainContentId,
                        getPath: nodeid =>
                        {
                            var ent = entityService.Get(nodeid);
                            return ent.Path.Split(Constants.CharArrays.Comma).Reverse();
                        },
                        publishedContentExists: i => publishedContentQuery.Content(i) != null);

                    //now we'll try to execute the expression
                    var nodeResult = publishedContentQuery.ContentSingleAtXPath(xpathResult);
                    if (nodeResult != null)
                        return nodeResult.Id;
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<NotFoundHandlerHelper,string>(ex, "Could not parse xpath expression: {ContentXPath}", errorPage.ContentXPath);
                    return null;
                }
            }
            return null;
        }

    }
}
