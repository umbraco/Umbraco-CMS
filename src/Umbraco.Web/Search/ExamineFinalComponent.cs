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
        }

        private void UmbracoModule_RouteAttempt(object sender, RoutableAttemptEventArgs e)
        {
            if (!_initialized)
            {
                lock (_locker)
                {
                    // double check lock, we must only do this once
                    if (!_initialized)
                    {
                        _initialized = true;

                        UmbracoModule.RouteAttempt -= UmbracoModule_RouteAttempt;

                        if (!_mainDom.IsMainDom) return;

                        var bootState = _syncBootStateAccessor.GetSyncBootState();

                        _examineManager.ConfigureIndexes(_mainDom, _logger);

                        // if it's a cold boot, rebuild all indexes including non-empty ones
                        // delay one minute since a cold boot also triggers nucache rebuilds
                        _indexRebuilder.RebuildIndexes(bootState != SyncBootState.ColdBoot, 60000);
                    }
                }
            }
            
        }

        public void Initialize()
        {
            UmbracoModule.RouteAttempt += UmbracoModule_RouteAttempt;
        }

        public void Terminate()
        {
        }
    }
}
