using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Configuration;
using Umbraco.Configuration;
using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.Web.Cache;
using Umbraco.Core.Configuration.Models;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace Umbraco.ModelsBuilder.Embedded
{
    // supports LiveAppData - but not PureLive
    public sealed class LiveModelsProvider
    {
        private static Mutex _mutex;
        private static int _req;
        private readonly ILogger<LiveModelsProvider> _logger;
        private readonly ModelsBuilderSettings _config;
        private readonly ModelsGenerator _modelGenerator;
        private readonly ModelsGenerationError _mbErrors;
        private readonly IHostingEnvironment _hostingEnvironment;

        // we do not manage pure live here
        internal bool IsEnabled => _config.ModelsMode.IsLiveNotPure();

        public LiveModelsProvider(ILogger<LiveModelsProvider> logger, IOptions<ModelsBuilderSettings> config, ModelsGenerator modelGenerator, ModelsGenerationError mbErrors, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            _modelGenerator = modelGenerator;
            _mbErrors = mbErrors;
            _hostingEnvironment = hostingEnvironment;
        }

        internal void Install()
        {
            // just be sure
            if (!IsEnabled)
                return;

            // initialize mutex
            // ApplicationId will look like "/LM/W3SVC/1/Root/AppName"
            // name is system-wide and must be less than 260 chars
            var name = _hostingEnvironment.ApplicationId + "/UmbracoLiveModelsProvider";

            _mutex = new Mutex(false, name); //TODO: Replace this with MainDom? Seems we now have 2x implementations of almost the same thing

            // anything changes, and we want to re-generate models.
            ContentTypeCacheRefresher.CacheUpdated += RequestModelsGeneration;
            DataTypeCacheRefresher.CacheUpdated += RequestModelsGeneration;

            // at the end of a request since we're restarting the pool
            // NOTE - this does NOT trigger - see module below
            //umbracoApplication.EndRequest += GenerateModelsIfRequested;
        }

        // NOTE
        // Using HttpContext Items fails because CacheUpdated triggers within
        // some asynchronous backend task where we seem to have no HttpContext.

        // So we use a static (non request-bound) var to register that models
        // need to be generated. Could be by another request. Anyway. We could
        // have collisions but... you know the risk.

        private void RequestModelsGeneration(object sender, EventArgs args)
        {
            //HttpContext.Current.Items[this] = true;
            _logger.LogDebug("Requested to generate models.");
            Interlocked.Exchange(ref _req, 1);
        }

        public void GenerateModelsIfRequested()
        {
            //if (HttpContext.Current.Items[this] == null) return;
            if (Interlocked.Exchange(ref _req, 0) == 0) return;

            // cannot use a simple lock here because we don't want another AppDomain
            // to generate while we do... and there could be 2 AppDomains if the app restarts.

            try
            {
                _logger.LogDebug("Generate models...");
                const int timeout = 2 * 60 * 1000; // 2 mins
                _mutex.WaitOne(timeout); // wait until it is safe, and acquire
                _logger.LogInformation("Generate models now.");
                GenerateModels();
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
                _mutex.ReleaseMutex(); // release
            }
        }

        private void GenerateModels()
        {
            // EnableDllModels will recycle the app domain - but this request will end properly
            _modelGenerator.GenerateModels();
        }

        public void AppEndRequest(HttpContext context)
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
