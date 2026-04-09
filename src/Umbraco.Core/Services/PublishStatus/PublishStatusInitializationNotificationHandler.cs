using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly IDocumentPublishStatusManagementService _documentPublishStatusManagementService;
    private readonly IElementPublishStatusManagementService _elementPublishStatusManagementService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishStatusInitializationNotificationHandler"/> class.
    /// </summary>
    /// <param name="runtimeState">The runtime state.</param>
    /// <param name="documentPublishStatusManagementService">The document publish status management service.</param>
    /// <param name="elementPublishStatusManagementService">The element publish status management service.</param>
    public PublishStatusInitializationNotificationHandler(
        IRuntimeState runtimeState,
        IDocumentPublishStatusManagementService documentPublishStatusManagementService,
        IElementPublishStatusManagementService elementPublishStatusManagementService)
    {
        _runtimeState = runtimeState;
        _documentPublishStatusManagementService = documentPublishStatusManagementService;
        _elementPublishStatusManagementService = elementPublishStatusManagementService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishStatusInitializationNotificationHandler"/> class.
    /// </summary>
    /// <param name="runtimeState">The runtime state.</param>
    /// <param name="publishStatusManagementService">The publish status management service.</param>
#pragma warning disable CS0618 // Type or member is obsolete
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public PublishStatusInitializationNotificationHandler(
        IRuntimeState runtimeState,
        IPublishStatusManagementService publishStatusManagementService)
        : this(
            runtimeState,
            publishStatusManagementService,
            StaticServiceProvider.Instance.GetRequiredService<IElementPublishStatusManagementService>())
    {
    }
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishStatusInitializationNotificationHandler"/> class.
    /// </summary>
    /// <param name="runtimeState">The runtime state.</param>
    /// <param name="publishStatusManagementService">The publish status management service.</param>
    /// <param name="documentPublishStatusManagementService">The document publish status management service.</param>
    /// <param name="elementPublishStatusManagementService">The element publish status management service.</param>
#pragma warning disable CS0618 // Type or member is obsolete
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public PublishStatusInitializationNotificationHandler(
        IRuntimeState runtimeState,
        IPublishStatusManagementService publishStatusManagementService,
        IDocumentPublishStatusManagementService documentPublishStatusManagementService,
        IElementPublishStatusManagementService elementPublishStatusManagementService)
        : this(
            runtimeState,
            documentPublishStatusManagementService,
            elementPublishStatusManagementService)
    {
    }
#pragma warning restore CS0618 // Type or member is obsolete

    /// <inheritdoc />
    public async Task HandleAsync(PostRuntimePremigrationsUpgradeNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level < RuntimeLevel.Upgrade)
        {
            return;
        }

        await _documentPublishStatusManagementService.InitializeAsync(cancellationToken);
        await _elementPublishStatusManagementService.InitializeAsync(cancellationToken);
    }
}
