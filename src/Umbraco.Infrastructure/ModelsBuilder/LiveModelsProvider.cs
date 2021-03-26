using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder
{
    // supports LiveAppData - but not PureLive
    public sealed class LiveModelsProvider : INotificationHandler<UmbracoApplicationStarting>,
        INotificationHandler<UmbracoRequestEnd>,
        INotificationHandler<ContentTypeCacheRefresherNotification>,
        INotificationHandler<DataTypeCacheRefresherNotification>
    {
        private static int s_req;
        private readonly ILogger<LiveModelsProvider> _logger;
        private readonly ModelsBuilderSettings _config;
        private readonly ModelsGenerator _modelGenerator;
        private readonly ModelsGenerationError _mbErrors;
        private readonly IMainDom _mainDom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveModelsProvider"/> class.
        /// </summary>
        public LiveModelsProvider(
            ILogger<LiveModelsProvider> logger,
            IOptions<ModelsBuilderSettings> config,
            ModelsGenerator modelGenerator,
            ModelsGenerationError mbErrors,
            IMainDom mainDom)
        {
            _logger = logger;
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            _modelGenerator = modelGenerator;
            _mbErrors = mbErrors;
            _mainDom = mainDom;
        }

        // we do not manage pure live here
        internal bool IsEnabled => _config.ModelsMode.IsLiveNotPure();

        /// <summary>
        /// Handles the <see cref="UmbracoApplicationStarting"/> notification
        /// </summary>
        public void Handle(UmbracoApplicationStarting notification) => Install();

        private void Install()
        {
            // don't run if not enabled
            if (!IsEnabled)
            {
                return;
            }
        }

        // NOTE
        // CacheUpdated triggers within some asynchronous backend task where
        // we have no HttpContext. So we use a static (non request-bound)
        // var to register that models
        // need to be generated. Could be by another request. Anyway. We could
        // have collisions but... you know the risk.

        private void RequestModelsGeneration()
        {
            if (!_mainDom.IsMainDom)
            {
                return;
            }

            _logger.LogDebug("Requested to generate models.");
            Interlocked.Exchange(ref s_req, 1);
        }

        private void GenerateModelsIfRequested()
        {
            if (Interlocked.Exchange(ref s_req, 0) == 0)
            {
                return;
            }

            // cannot proceed unless we are MainDom
            if (_mainDom.IsMainDom)
            {
                try
                {
                    _logger.LogDebug("Generate models...");
                    _logger.LogInformation("Generate models now.");
                    _modelGenerator.GenerateModels();
                    _mbErrors.Clear();
                    _logger.LogInformation("Generated.");
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning("Timeout, models were NOT generated.");
                }
                catch (Exception e)
                {
                    _mbErrors.Report("Failed to build Live models.", e);
                    _logger.LogError("Failed to generate models.", e);
                }
            }
            else
            {
                // this will only occur if this appdomain was MainDom and it has
                // been released while trying to regenerate models.
                _logger.LogWarning("Cannot generate models while app is shutting down");
            }
        }

        public void Handle(UmbracoRequestEnd notification)
        {
            if (IsEnabled && _mainDom.IsMainDom)
            {
                GenerateModelsIfRequested();
            }
        }

        public void Handle(ContentTypeCacheRefresherNotification notification) => RequestModelsGeneration();

        public void Handle(DataTypeCacheRefresherNotification notification) => RequestModelsGeneration();
    }
}
