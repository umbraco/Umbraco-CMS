using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Extensions;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.Web.Cache;

namespace Umbraco.ModelsBuilder.Embedded
{
    // supports LiveAppData - but not PureLive
    public sealed class LiveModelsProvider
    {
        private static Mutex s_mutex;
        private static int s_req;
        private readonly ILogger<LiveModelsProvider> _logger;
        private readonly ModelsBuilderSettings _config;
        private readonly ModelsGenerator _modelGenerator;
        private readonly ModelsGenerationError _mbErrors;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveModelsProvider"/> class.
        /// </summary>
        public LiveModelsProvider(ILogger<LiveModelsProvider> logger, IOptions<ModelsBuilderSettings> config, ModelsGenerator modelGenerator, ModelsGenerationError mbErrors, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            _modelGenerator = modelGenerator;
            _mbErrors = mbErrors;
            _hostingEnvironment = hostingEnvironment;
        }

        // we do not manage pure live here
        internal bool IsEnabled => _config.ModelsMode.IsLiveNotPure();

        internal void Install()
        {
            // just be sure
            if (!IsEnabled)
            {
                return;
            }

            // initialize mutex
            // ApplicationId will look like "/LM/W3SVC/1/Root/AppName"
            // name is system-wide and must be less than 260 chars
            var name = _hostingEnvironment.ApplicationId + "/UmbracoLiveModelsProvider";

            s_mutex = new Mutex(false, name); //TODO: Replace this with MainDom? Seems we now have 2x implementations of almost the same thing

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
            Interlocked.Exchange(ref s_req, 1);
        }

        public void GenerateModelsIfRequested()
        {
            //if (HttpContext.Current.Items[this] == null) return;
            if (Interlocked.Exchange(ref s_req, 0) == 0) return;

            // cannot use a simple lock here because we don't want another AppDomain
            // to generate while we do... and there could be 2 AppDomains if the app restarts.

            try
            {
                _logger.LogDebug("Generate models...");
                const int timeout = 2 * 60 * 1000; // 2 mins
                s_mutex.WaitOne(timeout); // wait until it is safe, and acquire
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
                s_mutex.ReleaseMutex(); // release
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
