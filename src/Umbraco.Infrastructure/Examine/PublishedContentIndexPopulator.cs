using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Examine;

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
    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedContentIndexPopulator"/> class.
    /// </summary>
    /// <param name="logger">The logger used for logging information and errors.</param>
    /// <param name="contentService">The service used to manage Umbraco content.</param>
    /// <param name="umbracoDatabaseFactory">The factory for creating Umbraco database instances.</param>
    /// <param name="contentValueSetBuilder">The builder for creating value sets from published content.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public PublishedContentIndexPopulator(
        ILogger<PublishedContentIndexPopulator> logger,
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IPublishedContentValueSetBuilder contentValueSetBuilder)
        : base(logger, true, null, contentService, umbracoDatabaseFactory, contentValueSetBuilder)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedContentIndexPopulator"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{PublishedContentIndexPopulator}"/> used for logging operations.</param>
    /// <param name="contentService">The <see cref="IContentService"/> used to access and manage content items.</param>
    /// <param name="umbracoDatabaseFactory">The <see cref="IUmbracoDatabaseFactory"/> used to create Umbraco database connections.</param>
    /// <param name="contentValueSetBuilder">The <see cref="IPublishedContentValueSetBuilder"/> responsible for building value sets for published content.</param>
    /// <param name="indexingSettings">The <see cref="IOptionsMonitor{IndexingSettings}"/> providing access to indexing configuration settings.</param>
    public PublishedContentIndexPopulator(
        ILogger<PublishedContentIndexPopulator> logger,
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IPublishedContentValueSetBuilder contentValueSetBuilder,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(logger, true, null, contentService, umbracoDatabaseFactory, contentValueSetBuilder, indexingSettings)
    {
    }
}
