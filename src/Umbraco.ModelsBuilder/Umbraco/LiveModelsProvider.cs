using System;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.ModelsBuilder.Configuration;
using Umbraco.ModelsBuilder.Umbraco;
using Umbraco.Web.Cache;

// will install only if configuration says it needs to be installed
[assembly: PreApplicationStartMethod(typeof(LiveModelsProviderModule), "Install")]

namespace Umbraco.ModelsBuilder.Umbraco
{
    // supports LiveDll and LiveAppData - but not PureLive
    public sealed class LiveModelsProvider
    {
        private static UmbracoServices _umbracoServices;
        private static Mutex _mutex;
        private static int _req;

        internal static bool IsEnabled
        {
            get
            {
                var config = UmbracoConfig.For.ModelsBuilder();
                return config.ModelsMode.IsLiveNotPure();
                // we do not manage pure live here
            }
        }

        internal static void Install(UmbracoServices umbracoServices)
        {
            // just be sure
            if (!IsEnabled)
                return;

            _umbracoServices = umbracoServices;

            // initialize mutex
            // ApplicationId will look like "/LM/W3SVC/1/Root/AppName"
            // name is system-wide and must be less than 260 chars
            var name = HostingEnvironment.ApplicationID + "/UmbracoLiveModelsProvider";
            _mutex = new Mutex(false, name);

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

        private static void RequestModelsGeneration(object sender, EventArgs args)
        {
            //HttpContext.Current.Items[this] = true;
            Current.Logger.Debug<LiveModelsProvider>("Requested to generate models.");
            Interlocked.Exchange(ref _req, 1);
        }

        public static void GenerateModelsIfRequested(object sender, EventArgs args)
        {
            //if (HttpContext.Current.Items[this] == null) return;
            if (Interlocked.Exchange(ref _req, 0) == 0) return;

            // cannot use a simple lock here because we don't want another AppDomain
            // to generate while we do... and there could be 2 AppDomains if the app restarts.

            try
            {
                Current.Logger.Debug<LiveModelsProvider>("Generate models...");
                const int timeout = 2*60*1000; // 2 mins
                _mutex.WaitOne(timeout); // wait until it is safe, and acquire
                Current.Logger.Info<LiveModelsProvider>("Generate models now.");
                GenerateModels();
                ModelsGenerationError.Clear();
                Current.Logger.Info<LiveModelsProvider>("Generated.");
            }
            catch (TimeoutException)
            {
                Current.Logger.Warn<LiveModelsProvider>("Timeout, models were NOT generated.");
            }
            catch (Exception e)
            {
                ModelsGenerationError.Report("Failed to build Live models.", e);
                Current.Logger.Error<LiveModelsProvider>("Failed to generate models.", e);
            }
            finally
            {
                _mutex.ReleaseMutex(); // release
            }
        }

        private static void GenerateModels()
        {
            var modelsDirectory = UmbracoConfig.For.ModelsBuilder().ModelsDirectory;

            var bin = HostingEnvironment.MapPath("~/bin");
            if (bin == null)
                throw new Exception("Panic: bin is null.");

            var config = UmbracoConfig.For.ModelsBuilder();

            // EnableDllModels will recycle the app domain - but this request will end properly
            ModelsBuilderBackOfficeController.GenerateModels(_umbracoServices, modelsDirectory, config.ModelsMode.IsAnyDll() ? bin : null);
        }
    }

    // have to do this because it's the only way to subscribe to EndRequest,
    // module is installed by assembly attribute at the top of this file
    public class LiveModelsProviderModule : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.EndRequest += LiveModelsProvider.GenerateModelsIfRequested;
        }

        public void Dispose()
        {
            // nothing
        }

        public static void Install()
        {
            if (!LiveModelsProvider.IsEnabled)
                return;

            HttpApplication.RegisterModule(typeof(LiveModelsProviderModule));
        }
    }
}
