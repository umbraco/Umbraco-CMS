using System;
using System.Threading;
using System.Web.Hosting;
using Umbraco.Core.Logging;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.ModelsBuilder.Embedded.Configuration;
using Umbraco.Web.Cache;

namespace Umbraco.ModelsBuilder.Embedded
{
    // supports LiveAppData - but not PureLive
    public sealed class LiveModelsProvider
    {
        private static Mutex _mutex;
        private static int _req;
        private readonly ILogger _logger;
        private readonly IModelsBuilderConfig _config;
        private readonly ModelsGenerator _modelGenerator;
        private readonly ModelsGenerationError _mbErrors;

        // we do not manage pure live here
        internal bool IsEnabled => _config.ModelsMode.IsLiveNotPure();

        public LiveModelsProvider(ILogger logger, IModelsBuilderConfig config, ModelsGenerator modelGenerator, ModelsGenerationError mbErrors)
        {
            _logger = logger;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _modelGenerator = modelGenerator;
            _mbErrors = mbErrors;
        }

        internal void Install()
        {
            // just be sure
            if (!IsEnabled)
                return;

            // initialize mutex
            // ApplicationId will look like "/LM/W3SVC/1/Root/AppName"
            // name is system-wide and must be less than 260 chars
            var name = HostingEnvironment.ApplicationID + "/UmbracoLiveModelsProvider";

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
            _logger.Debug<LiveModelsProvider>("Requested to generate models.");
            Interlocked.Exchange(ref _req, 1);
        }

        public void GenerateModelsIfRequested(object sender, EventArgs args)
        {
            //if (HttpContext.Current.Items[this] == null) return;
            if (Interlocked.Exchange(ref _req, 0) == 0) return;

            // cannot use a simple lock here because we don't want another AppDomain
            // to generate while we do... and there could be 2 AppDomains if the app restarts.

            try
            {
                _logger.Debug<LiveModelsProvider>("Generate models...");
                const int timeout = 2 * 60 * 1000; // 2 mins
                _mutex.WaitOne(timeout); // wait until it is safe, and acquire
                _logger.Info<LiveModelsProvider>("Generate models now.");
                GenerateModels();
                _mbErrors.Clear();
                _logger.Info<LiveModelsProvider>("Generated.");
            }
            catch (TimeoutException)
            {
                _logger.Warn<LiveModelsProvider>("Timeout, models were NOT generated.");
            }
            catch (Exception e)
            {
                _mbErrors.Report("Failed to build Live models.", e);
                _logger.Error<LiveModelsProvider>(e, "Failed to generate models.");
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


    }
}
