using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
using Umbraco.Extensions;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.Web.Cache;
using Umbraco.Web.Common.Lifetime;

namespace Umbraco.ModelsBuilder.Embedded
{
    // supports LiveAppData - but not PureLive
    public sealed class LiveModelsProvider : INotificationHandler<UmbracoApplicationStarting>
    {
        private static int s_req;
        private readonly ILogger<LiveModelsProvider> _logger;
        private readonly ModelsBuilderSettings _config;
        private readonly ModelsGenerator _modelGenerator;
        private readonly ModelsGenerationError _mbErrors;
        private readonly IUmbracoRequestLifetime _umbracoRequestLifetime;
        private readonly IMainDom _mainDom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveModelsProvider"/> class.
        /// </summary>
        public LiveModelsProvider(
            ILogger<LiveModelsProvider> logger,
            IOptions<ModelsBuilderSettings> config,
            ModelsGenerator modelGenerator,
            ModelsGenerationError mbErrors,
            IUmbracoRequestLifetime umbracoRequestLifetime,
            IMainDom mainDom)
        {
            _logger = logger;
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            _modelGenerator = modelGenerator;
            _mbErrors = mbErrors;
            _umbracoRequestLifetime = umbracoRequestLifetime;
            _mainDom = mainDom;
        }

        // we do not manage pure live here
        internal bool IsEnabled => _config.ModelsMode.IsLiveNotPure();

        /// <summary>
        /// Handles the <see cref="UmbracoApplicationStarting"/> notification
        /// </summary>
        public void Handle(UmbracoApplicationStarting notification)
        {
            Install();
        }

        private void Install()
        {
            // don't run if not enabled
            if (!IsEnabled)
            {
                return;
            }

            // Must register with maindom in order to function.
            // If registration is not successful then events are not bound
            // and we also don't generate models.
            _mainDom.Register(() =>
            {
                _umbracoRequestLifetime.RequestEnd += (sender, context) => AppEndRequest(context);

                // anything changes, and we want to re-generate models.
                ContentTypeCacheRefresher.CacheUpdated += RequestModelsGeneration;
                DataTypeCacheRefresher.CacheUpdated += RequestModelsGeneration;
            });
        }

        // NOTE
        // CacheUpdated triggers within some asynchronous backend task where
        // we have no HttpContext. So we use a static (non request-bound)
        // var to register that models
        // need to be generated. Could be by another request. Anyway. We could
        // have collisions but... you know the risk.

        private void RequestModelsGeneration(object sender, EventArgs args)
        {
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

        private void AppEndRequest(HttpContext context)
        {
            if (context.Request.IsClientSideRequest())
            {
                return;
            }

            if (!IsEnabled)
            {
                return;
            }

            GenerateModelsIfRequested();
        }
    }
}
