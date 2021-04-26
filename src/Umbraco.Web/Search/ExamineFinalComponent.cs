using Examine;
using Umbraco.Core.Logging;
using Umbraco.Examine;
using Umbraco.Core.Composing;
using Umbraco.Core;
using Umbraco.Core.Sync;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Search
{

    /// <summary>
    /// Executes after all other examine components have executed
    /// </summary>
    public sealed class ExamineFinalComponent : IComponent
    {   
        private readonly IProfilingLogger _logger;
        private readonly IExamineManager _examineManager;
        BackgroundIndexRebuilder _indexRebuilder;
        private readonly IMainDom _mainDom;
        private readonly ISyncBootStateAccessor _syncBootStateAccessor;
        private readonly object _locker = new object();
        private bool _initialized = false;

        public ExamineFinalComponent(IProfilingLogger logger, IExamineManager examineManager, BackgroundIndexRebuilder indexRebuilder, IMainDom mainDom, ISyncBootStateAccessor syncBootStateAccessor)
        {
            _logger = logger;
            _examineManager = examineManager;
            _indexRebuilder = indexRebuilder;
            _mainDom = mainDom;
            _syncBootStateAccessor = syncBootStateAccessor;

            // must add the handler in the ctor because it will be too late in Initialize
            // TODO: All of this boot synchronization for cold boot logic needs should be fixed in netcore
            _syncBootStateAccessor.Booting += _syncBootStateAccessor_Booting;
        }

        /// <summary>
        /// Once the boot state is known we can see if we require rebuilds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _syncBootStateAccessor_Booting(object sender, SyncBootState e)
        {
            UmbracoModule.RouteAttempt += UmbracoModule_RouteAttempt;
        }

        private void UmbracoModule_RouteAttempt(object sender, RoutableAttemptEventArgs e)
        {
            UmbracoModule.RouteAttempt -= UmbracoModule_RouteAttempt;

            if (!_initialized)
            {
                lock (_locker)
                {
                    // double check lock, we must only do this once
                    if (!_initialized)
                    {
                        if (!_mainDom.IsMainDom) return;

                        var bootState = _syncBootStateAccessor.GetSyncBootState();

                        _examineManager.ConfigureIndexes(_mainDom, _logger);

                        // if it's a cold boot, rebuild all indexes including non-empty ones
                        _indexRebuilder.RebuildIndexes(bootState != SyncBootState.ColdBoot, 0);
                    }
                }
            }
            
        }

        public void Initialize()
        {   
        }

        public void Terminate()
        {
            _syncBootStateAccessor.Booting -= _syncBootStateAccessor_Booting;
        }
    }
}
