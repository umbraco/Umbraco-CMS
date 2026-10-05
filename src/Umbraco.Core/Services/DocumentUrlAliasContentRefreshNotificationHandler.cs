using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Persists a document's URL aliases as part of the transaction that persists the document.
/// </summary>
/// <remarks>
/// <see cref="ContentRefreshNotification"/> is published by the document repository inside the content transaction,
/// before the entity's published state is reset, so the aliases are written under the same lock and commit as the
/// content they describe. The in-memory alias cache is updated afterwards by the content cache refresher on every
/// server, including the one that made the change.
/// </remarks>
public sealed class DocumentUrlAliasContentRefreshNotificationHandler : IDistributedCacheAsyncNotificationHandler<ContentRefreshNotification>
{
    private readonly IDocumentUrlAliasService _documentUrlAliasService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlAliasContentRefreshNotificationHandler"/> class.
    /// </summary>
    /// <param name="documentUrlAliasService">The document URL alias service.</param>
    public DocumentUrlAliasContentRefreshNotificationHandler(IDocumentUrlAliasService documentUrlAliasService)
        => _documentUrlAliasService = documentUrlAliasService;

    /// <inheritdoc/>
    public Task HandleAsync(ContentRefreshNotification notification, CancellationToken cancellationToken)
        => _documentUrlAliasService.IsInitialized
            ? _documentUrlAliasService.PersistAliasesAsync(notification.Entity)
            : Task.CompletedTask;
}
