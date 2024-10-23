using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Services;

public class DocumentUrlServiceInitializerNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IRuntimeState _runtimeState;

    public DocumentUrlServiceInitializerNotificationHandler(IDocumentUrlService documentUrlService, IRuntimeState runtimeState)
    {
        _documentUrlService = documentUrlService;
        _runtimeState = runtimeState;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
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
