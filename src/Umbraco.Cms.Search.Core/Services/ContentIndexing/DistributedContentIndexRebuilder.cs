using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Search.Core.Cache.Index;
using Umbraco.Cms.Search.Core.Configuration;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

internal sealed class DistributedContentIndexRebuilder : IDistributedContentIndexRebuilder
{
    private readonly RebuildIndexNotificationHandler _rebuildIndexNotificationHandler;
    private readonly IndexOptions _options;
    private readonly ILogger<DistributedContentIndexRebuilder> _logger;

    public DistributedContentIndexRebuilder(
        RebuildIndexNotificationHandler rebuildIndexNotificationHandler,
        IOptions<IndexOptions> options,
        ILogger<DistributedContentIndexRebuilder> logger)
    {
        _rebuildIndexNotificationHandler = rebuildIndexNotificationHandler;
        _logger = logger;
        _options = options.Value;
    }

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
