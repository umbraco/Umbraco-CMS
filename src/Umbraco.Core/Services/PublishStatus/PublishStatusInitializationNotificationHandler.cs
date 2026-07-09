using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Responsible for seeding the in-memory publish status cache at application's startup
/// by loading all data from the database.
/// </summary>
public sealed class PublishStatusInitializationNotificationHandler : INotificationAsyncHandler<PostRuntimePremigrationsUpgradeNotification>
{
    private readonly IRuntimeState _runtimeState;
    private readonly IPublishStatusManagementService _publishStatusManagementService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishStatusInitializationNotificationHandler"/> class.
    /// </summary>
    /// <param name="runtimeState">The runtime state.</param>
    /// <param name="publishStatusManagementService">The publish status management service.</param>
    public PublishStatusInitializationNotificationHandler(
        IRuntimeState runtimeState,
        IPublishStatusManagementService publishStatusManagementService)
    {
        _runtimeState = runtimeState;
        _publishStatusManagementService = publishStatusManagementService;
    }

    /// <inheritdoc />
    public async Task HandleAsync(PostRuntimePremigrationsUpgradeNotification notification, CancellationToken cancellationToken)
    {
        if(_runtimeState.Level < RuntimeLevel.Upgrade)
        {
            return;
        }

        await _publishStatusManagementService.InitializeAsync(cancellationToken);
    }
}
