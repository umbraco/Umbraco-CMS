using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Search.Examine.Configuration;

namespace Umbraco.Search.Indexing.Populators;

/// <summary>
///     Performs the data lookups required to rebuild a content index containing only published content
/// </summary>
/// <remarks>
///     The published (external) index will still rebuild just fine using the default <see cref="ContentIndexPopulator" />
///     which is what is used when rebuilding all indexes,
///     but this will be used when the single index is rebuilt and will go a little bit faster since the data query is more specific.
///     since the data query is more specific.
/// </remarks>
public class PublishedContentIndexPopulator : ContentIndexPopulator
{
    public PublishedContentIndexPopulator(
        ILogger<PublishedContentIndexPopulator> logger,
        IContentService contentService,
        ISearchProvider provider,
        IUmbracoIndexesConfiguration configuration,
        IUmbracoDatabaseFactory umbracoDatabaseFactory)
        : base(logger,provider, true, null, contentService,configuration, umbracoDatabaseFactory)
    {
    }
}
