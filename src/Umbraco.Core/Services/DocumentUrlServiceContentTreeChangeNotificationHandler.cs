using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Handles <see cref="ContentTreeChangeNotification"/> to persist URL segments to the database on the originating
/// server. This fires post-commit (during scope disposal) before the cache instruction is delivered to other servers,
/// ensuring URL segments are in the database before any server processes the instruction. URL aliases are persisted
/// inside the content transaction by <see cref="DocumentUrlAliasContentRefreshNotificationHandler"/>.
/// </summary>
public class DocumentUrlServiceContentTreeChangeNotificationHandler
    : IDistributedCacheAsyncNotificationHandler<ContentTreeChangeNotification>
{
    private readonly IDocumentUrlService _documentUrlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlServiceContentTreeChangeNotificationHandler"/> class.
    /// </summary>
    public DocumentUrlServiceContentTreeChangeNotificationHandler(
        IDocumentUrlService documentUrlService,
        IDocumentUrlAliasService documentUrlAliasService)
    {
        _documentUrlService = documentUrlService;
    }

    /// <inheritdoc/>
    public async Task HandleAsync(ContentTreeChangeNotification notification, CancellationToken cancellationToken)
    {
        if (_documentUrlService.IsInitialized is false)
        {
            return;
        }

        var refreshNodeItems = new List<IContent>();

        foreach (TreeChange<IContent> change in notification.Changes)
        {
            if (change.ChangeTypes.HasType(TreeChangeTypes.RefreshNode))
            {
                refreshNodeItems.Add(change.Item);
            }

            if (change.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
            {
                await _documentUrlService.CreateOrUpdateUrlSegmentsWithDescendantsAsync(change.Item.Key);
            }
        }

        if (refreshNodeItems.Count > 0)
        {
            await _documentUrlService.CreateOrUpdateUrlSegmentsAsync(refreshNodeItems);
        }
    }
}
