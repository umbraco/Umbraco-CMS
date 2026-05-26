using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Handles <see cref="ContentTreeChangeNotification"/> to persist URL segments and aliases to the database
/// on the originating server. This fires post-commit (during scope disposal) before the cache instruction
/// is delivered to other servers, ensuring URL data is in the database before any server processes the instruction.
/// </summary>
public class DocumentUrlServiceContentTreeChangeNotificationHandler
    : INotificationAsyncHandler<ContentTreeChangeNotification>
{
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IDocumentUrlAliasService _documentUrlAliasService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlServiceContentTreeChangeNotificationHandler"/> class.
    /// </summary>
    public DocumentUrlServiceContentTreeChangeNotificationHandler(
        IDocumentUrlService documentUrlService,
        IDocumentUrlAliasService documentUrlAliasService)
    {
        _documentUrlService = documentUrlService;
        _documentUrlAliasService = documentUrlAliasService;
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
                await _documentUrlAliasService.CreateOrUpdateAliasesWithDescendantsAsync(change.Item.Key);
            }
        }

        if (refreshNodeItems.Count > 0)
        {
            await _documentUrlService.CreateOrUpdateUrlSegmentsAsync(refreshNodeItems);

            foreach (IContent item in refreshNodeItems)
            {
                await _documentUrlAliasService.CreateOrUpdateAliasesAsync(item.Key);
            }
        }
    }
}
