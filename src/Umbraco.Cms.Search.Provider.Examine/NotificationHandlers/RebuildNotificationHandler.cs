using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Models.Configuration;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Search.Provider.Examine.Services;
using IndexOptions = Umbraco.Cms.Search.Core.Configuration.IndexOptions;

namespace Umbraco.Cms.Search.Provider.Examine.NotificationHandlers;

/// <summary>
/// On application startup, rebuilds any registered index whose active physical Lucene index does not yet exist.
/// </summary>
public class RebuildNotificationHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly IExamineManager _examineManager;
    private readonly IActiveIndexManager _activeIndexManager;
    private readonly IContentIndexingService _contentIndexingService;
    private readonly ILogger<RebuildNotificationHandler> _logger;
    private readonly IOriginProvider _originProvider;
    private readonly IndexOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildNotificationHandler"/> class.
    /// </summary>
    /// <param name="examineManager">The manager used to check whether an index's active physical index already exists.</param>
    /// <param name="activeIndexManager">The manager used to resolve an index alias to its currently active physical index name.</param>
    /// <param name="contentIndexingService">The service used to trigger a rebuild of a missing index.</param>
    /// <param name="options">The options listing the registered content indexes to check on startup.</param>
    /// <param name="logger">The logger used to record which indexes are being rebuilt.</param>
    /// <param name="originProvider">The provider used to obtain the current server origin for the rebuild.</param>
    public RebuildNotificationHandler(
        IExamineManager examineManager,
        IActiveIndexManager activeIndexManager,
        IContentIndexingService contentIndexingService,
        IOptions<IndexOptions> options,
        ILogger<RebuildNotificationHandler> logger,
        IOriginProvider originProvider)
    {
        _examineManager = examineManager;
        _activeIndexManager = activeIndexManager;
        _contentIndexingService = contentIndexingService;
        _logger = logger;
        _originProvider = originProvider;
        _options = options.Value;
    }

    /// <inheritdoc />
    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        _logger.LogInformation("Boot detected, determining indexes to rebuild");
        foreach (ContentIndexRegistration indexRegistration in _options.GetContentIndexRegistrations())
        {
            var activePhysicalName = _activeIndexManager.ResolveActiveIndexName(indexRegistration.IndexAlias);

            if (_examineManager.TryGetIndex(activePhysicalName, out IIndex? index))
            {
                // Check if active physical index exists, if it does, we can skip rebuilding
                if (index.IndexExists())
                {
                    continue;
                }
            }
            else
            {
                // Not a registered examine index, don't rebuild from here.
                continue;
            }

            _logger.LogInformation("Rebuilding index {IndexRegistrationIndexAlias}", indexRegistration.IndexAlias);
            _contentIndexingService.Rebuild(indexRegistration.IndexAlias, _originProvider.GetCurrent());
        }
    }
}
