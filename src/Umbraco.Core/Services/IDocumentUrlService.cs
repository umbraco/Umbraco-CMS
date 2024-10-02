using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.Services;

public interface IDocumentUrlService
{
    /// <summary>
    /// Initializes the service and ensure the content in the database is correct with the current configuration.
    /// </summary>
    /// <param name="forceEmpty"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InitAsync(bool forceEmpty, CancellationToken cancellationToken);

    Task RebuildAllUrlsAsync();
    /// <summary>
    /// Gets the Url from a document key, culture and segment. Preview urls are returned if isPreview is true.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDraft">Whether to get the url of the draft or published document.</param>
    /// <returns>The url of the document.</returns>
    string? GetUrlSegment(Guid documentKey, string culture, bool isDraft);

    Task CreateOrUpdateUrlSegmentsAsync(IEnumerable<IContent> documents);

    Task DeleteUrlsFromCacheAsync(IEnumerable<Guid> documentKeys);

    Guid? GetDocumentKeyByRoute(string route, string? culture, int? documentStartNodeId, bool isDraft);
    Task<IEnumerable<UrlInfo>> ListUrlsAsync(Guid contentKey);
    Task CreateOrUpdateUrlSegmentsWithDescendantsAsync(Guid key);
    Task CreateOrUpdateUrlSegmentsAsync(Guid key);
    string GetLegacyRouteFormat(Guid key, string? culture, bool isDraft);

    bool HasAny();
}
