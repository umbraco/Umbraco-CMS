using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Extensions;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.Web.Cache;
using Umbraco.Web.Common.Lifetime;

namespace Umbraco.ModelsBuilder.Embedded
{
    // supports LiveAppData - but not PureLive
    public sealed class LiveModelsProvider : INotificationHandler<UmbracoApplicationStarting>
    {
        private static Mutex s_mutex;
        private static int s_req;
        private readonly ILogger<LiveModelsProvider> _logger;
        private readonly ModelsBuilderSettings _config;
        private readonly ModelsGenerator _modelGenerator;
        private readonly ModelsGenerationError _mbErrors;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUmbracoRequestLifetime _umbracoRequestLifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveModelsProvider"/> class.
        /// </summary>
        public LiveModelsProvider(
            ILogger<LiveModelsProvider> logger,
            IOptions<ModelsBuilderSettings> config,
            ModelsGenerator modelGenerator,
            ModelsGenerationError mbErrors,
            IHostingEnvironment hostingEnvironment,
            IUmbracoRequestLifetime umbracoRequestLifetime)
        {
            _logger = logger;
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            _modelGenerator = modelGenerator;
            _mbErrors = mbErrors;
            _hostingEnvironment = hostingEnvironment;
            _umbracoRequestLifetime = umbracoRequestLifetime;
        }

        // we do not manage pure live here
        internal bool IsEnabled => _config.ModelsMode.IsLiveNotPure();

        /// <summary>
        /// Handles the <see cref="UmbracoApplicationStarting"/> notification
        /// </summary>
        public Task HandleAsync(UmbracoApplicationStarting notification, CancellationToken cancellationToken)
        {
            Install();
            return Task.CompletedTask;
        }

        private void Install()
        {
            // don't run if not enabled
            if (!IsEnabled)
            {
                return;
            }

            _umbracoRequestLifetime.RequestEnd += (sender, context) => AppEndRequest(context);

            // initialize mutex
            // ApplicationId will look like "/LM/W3SVC/1/Root/AppName"
            // name is system-wide and must be less than 260 chars
            var name = _hostingEnvironment.ApplicationId + "/UmbracoLiveModelsProvider";

            s_mutex = new Mutex(false, name); //TODO: Replace this with MainDom? Seems we now have 2x implementations of almost the same thing

            // anything changes, and we want to re-generate models.
            ContentTypeCacheRefresher.CacheUpdated += RequestModelsGeneration;
            DataTypeCacheRefresher.CacheUpdated += RequestModelsGeneration;
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

            try
            {
                _logger.LogDebug("Generate models...");
                const int timeout = 2 * 60 * 1000; // 2 mins
                s_mutex.WaitOne(timeout); // wait until it is safe, and acquire
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
            finally
            {
                s_mutex.ReleaseMutex(); // release
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
