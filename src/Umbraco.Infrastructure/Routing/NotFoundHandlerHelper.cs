using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Used to determine the node to display when content is not found based on the configured error404 elements in
///     umbracoSettings.config
/// </summary>
internal sealed class NotFoundHandlerHelper
{
    internal static int? GetCurrentNotFoundPageId(
        ContentErrorPage[] error404Collection,
        IEntityService entityService,
        IPublishedContentQuery publishedContentQuery,
        string? errorCulture,
        int? domainContentId)
    {
        if (error404Collection.Length > 1)
        {
            // test if a 404 page exists with current culture thread
            ContentErrorPage? cultureErr =
                error404Collection.FirstOrDefault(x => x.Culture.InvariantEquals(errorCulture))
                ?? error404Collection.FirstOrDefault(x => x.Culture == "default"); // there should be a default one!

            if (cultureErr != null)
            {
                return GetContentIdFromErrorPageConfig(cultureErr, entityService, publishedContentQuery, domainContentId);
            }
        }
        else if (error404Collection.Length == 1)
        {
            return GetContentIdFromErrorPageConfig(error404Collection.First(), entityService, publishedContentQuery, domainContentId);
        }

        return null;
    }

    /// <summary>
    ///     Returns the content id based on the configured ContentErrorPage section.
    /// </summary>
    internal static int? GetContentIdFromErrorPageConfig(
        ContentErrorPage errorPage,
        IEntityService entityService,
        IPublishedContentQuery publishedContentQuery,
        int? domainContentId)
    {
        if (errorPage.HasContentId)
        {
            return errorPage.ContentId;
        }

        if (errorPage.HasContentKey)
        {
            // need to get the Id for the GUID
            // TODO: When we start storing GUIDs into the IPublishedContent, then we won't have to look this up
            // but until then we need to look it up in the db. For now we've implemented a cached service for
            // converting Int -> Guid and vice versa.
            Attempt<int> found = entityService.GetId(errorPage.ContentKey, UmbracoObjectTypes.Document);
            if (found.Success)
            {
                return found.Result;
            }

            return null;
        }

        return null;
    }
}
