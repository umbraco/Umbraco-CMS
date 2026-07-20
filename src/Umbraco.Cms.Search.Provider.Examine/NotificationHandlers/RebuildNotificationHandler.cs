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

public class RebuildNotificationHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly IExamineManager _examineManager;
    private readonly IActiveIndexManager _activeIndexManager;
    private readonly IContentIndexingService _contentIndexingService;
    private readonly ILogger<RebuildNotificationHandler> _logger;
    private readonly IOriginProvider _originProvider;
    private readonly IndexOptions _options;

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
