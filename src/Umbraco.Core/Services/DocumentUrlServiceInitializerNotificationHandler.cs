using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Handles the application starting notification to initialize the document URL service.
/// </summary>
public class DocumentUrlServiceInitializerNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
{
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlServiceInitializerNotificationHandler"/> class.
    /// </summary>
    /// <param name="documentUrlService">The document URL service.</param>
    /// <param name="runtimeState">The runtime state.</param>
    public DocumentUrlServiceInitializerNotificationHandler(IDocumentUrlService documentUrlService, IRuntimeState runtimeState)
    {
        _documentUrlService = documentUrlService;
        _runtimeState = runtimeState;
    }

    /// <inheritdoc />
    public async Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level == RuntimeLevel.Upgrade)
        {
            //Special case on the first upgrade, as the database is not ready yet.
            return;
        }

        await _documentUrlService.InitAsync(
            _runtimeState.Level <= RuntimeLevel.Install,
            cancellationToken);
    }
}
