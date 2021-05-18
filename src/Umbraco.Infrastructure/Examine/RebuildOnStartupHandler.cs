using System;
using System.Threading;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Examine
{
    /// <summary>
    /// Handles how the indexes are rebuilt on startup
    /// </summary>
    /// <remarks>
    /// On the first HTTP request this will rebuild the Examine indexes if they are empty.
    /// If it is a cold boot, they are all rebuilt.
    /// </remarks>
    public sealed class RebuildOnStartupHandler : INotificationHandler<UmbracoRequestBeginNotification>
    {
        private readonly ISyncBootStateAccessor _syncBootStateAccessor;
        private readonly ExamineIndexRebuilder _backgroundIndexRebuilder;

        // These must be static because notification handlers are transient.
        // this does unfortunatley mean that one RebuildOnStartupHandler instance
        // will be created for each front-end request even though we only use the first one.
        // TODO: Is there a better way to acheive this without allocating? We cannot remove
        // a handler from the notification system. It's not a huge deal but would be better
        // with less objects.
        private static bool _isReady;
        private static bool _isReadSet;
        private static object _isReadyLock;

        public RebuildOnStartupHandler(
            ISyncBootStateAccessor syncBootStateAccessor,
            ExamineIndexRebuilder backgroundIndexRebuilder)
        {
            _syncBootStateAccessor = syncBootStateAccessor;
            _backgroundIndexRebuilder = backgroundIndexRebuilder;
        }

        /// <summary>
        /// On first http request schedule an index rebuild for any empty indexes (or all if it's a cold boot)
        /// </summary>
        /// <param name="notification"></param>
        public void Handle(UmbracoRequestBeginNotification notification)
            => LazyInitializer.EnsureInitialized(
                ref _isReady,
                ref _isReadSet,
                ref _isReadyLock,
                () =>
                {
                    SyncBootState bootState = _syncBootStateAccessor.GetSyncBootState();

                    _backgroundIndexRebuilder.RebuildIndexes(
                        // if it's not a cold boot, only rebuild empty ones
                        bootState != SyncBootState.ColdBoot,
                        TimeSpan.FromMinutes(1));

                    return true;
                });
    }
}
