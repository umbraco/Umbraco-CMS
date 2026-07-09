using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Notification handler to initialize the <see cref="IDocumentUrlAliasService"/> on application startup.
/// </summary>
public class DocumentUrlAliasServiceInitializerNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
{
    private readonly IDocumentUrlAliasService _documentUrlAliasService;
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlAliasServiceInitializerNotificationHandler"/> class.
    /// </summary>
    public DocumentUrlAliasServiceInitializerNotificationHandler(
        IDocumentUrlAliasService documentUrlAliasService,
        IRuntimeState runtimeState)
    {
        _documentUrlAliasService = documentUrlAliasService;
        _runtimeState = runtimeState;
    }

    /// <inheritdoc/>
    public async Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level == RuntimeLevel.Upgrade)
        {
            // Special case on the first upgrade, as the database is not ready yet.
            return;
        }

        await _documentUrlAliasService.InitAsync(
            _runtimeState.Level <= RuntimeLevel.Install,
            cancellationToken);
    }
}
