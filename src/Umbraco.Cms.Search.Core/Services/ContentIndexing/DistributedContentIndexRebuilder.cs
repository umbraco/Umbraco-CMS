using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Search.Core.Cache.Index;
using Umbraco.Cms.Search.Core.Configuration;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Default implementation of <see cref="IDistributedContentIndexRebuilder"/>.
/// </summary>
internal sealed class DistributedContentIndexRebuilder : IDistributedContentIndexRebuilder
{
    private readonly RebuildIndexNotificationHandler _rebuildIndexNotificationHandler;
    private readonly IndexOptions _options;
    private readonly ILogger<DistributedContentIndexRebuilder> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedContentIndexRebuilder"/> class.
    /// </summary>
    /// <param name="rebuildIndexNotificationHandler">The handler used to broadcast the rebuild request to all servers.</param>
    /// <param name="options">The registered content index configuration.</param>
    /// <param name="logger">The logger used to record when no registration is found for the requested index.</param>
    public DistributedContentIndexRebuilder(
        RebuildIndexNotificationHandler rebuildIndexNotificationHandler,
        IOptions<IndexOptions> options,
        ILogger<DistributedContentIndexRebuilder> logger)
    {
        _rebuildIndexNotificationHandler = rebuildIndexNotificationHandler;
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public bool Rebuild(string indexAlias)
    {
        if (_options
                .GetContentIndexRegistrations()
                .Select(registration => registration.IndexAlias)
                .Contains(indexAlias) is false)
        {
            _logger.LogError("No index registration found for index with alias: {indexAlias} - skipping the reindex operation.", indexAlias);
            return false;
        }

        _rebuildIndexNotificationHandler.Handle([indexAlias]);
        return true;
    }
}
