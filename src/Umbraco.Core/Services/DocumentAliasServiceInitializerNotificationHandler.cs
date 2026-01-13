using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Notification handler to initialize the <see cref="IDocumentAliasService"/> on application startup.
/// </summary>
public class DocumentAliasServiceInitializerNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
{
    private readonly IDocumentAliasService _documentAliasService;
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentAliasServiceInitializerNotificationHandler"/> class.
    /// </summary>
    public DocumentAliasServiceInitializerNotificationHandler(
        IDocumentAliasService documentAliasService,
        IRuntimeState runtimeState)
    {
        _documentAliasService = documentAliasService;
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

        await _documentAliasService.InitAsync(
            _runtimeState.Level <= RuntimeLevel.Install,
            cancellationToken);
    }
}
