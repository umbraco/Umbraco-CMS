using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Handles how the indexes are rebuilt after startup.
/// </summary>
/// <remarks>
///     Once the application has fully started this rebuilds the Examine indexes if they are empty.
///     If it is a cold boot, they are all rebuilt.
/// </remarks>
public class RebuildOnStartedHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    // The notification is published again on restart, but the indexes only need to be
    // considered for rebuilding once per application lifetime.
    private static int _hasRun;

    private readonly ISyncBootStateAccessor _syncBootStateAccessor;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Examine.RebuildOnStartedHandler"/> class, responsible for handling index rebuilds during application startup.
    /// </summary>
    /// <param name="syncBootStateAccessor">Provides access to the application's synchronous boot state, used to determine if the system is ready for index rebuilding.</param>
    /// <param name="indexRebuilder">The service responsible for rebuilding Examine indexes.</param>
    /// <param name="runtimeState">Provides information about the current runtime state of the Umbraco application.</param>
    public RebuildOnStartedHandler(
        ISyncBootStateAccessor syncBootStateAccessor,
        IIndexRebuilder indexRebuilder,
        IRuntimeState runtimeState)
    {
        _syncBootStateAccessor = syncBootStateAccessor;
        _indexRebuilder = indexRebuilder;
        _runtimeState = runtimeState;
    }

    /// <summary>
    ///     Once the application has fully started, schedule an index rebuild for any empty indexes (or all if it's a cold boot).
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _hasRun, 1, 0) != 0)
        {
            return;
        }

        SyncBootState bootState = _syncBootStateAccessor.GetSyncBootState();

        // if it's not a cold boot, only rebuild empty ones
        await _indexRebuilder.RebuildIndexesAsync(
            bootState != SyncBootState.ColdBoot,
            TimeSpan.FromMinutes(1));
    }
}
