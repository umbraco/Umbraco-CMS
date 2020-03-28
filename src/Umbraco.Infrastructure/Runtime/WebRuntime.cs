using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.Runtime
{
    /// <summary>
    /// Represents the Web Umbraco runtime.
    /// </summary>
    /// <remarks>On top of CoreRuntime, handles all of the web-related runtime aspects of Umbraco.</remarks>
    public class WebRuntime : CoreRuntime
    {
        private readonly IRequestCache _requestCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRuntime"/> class.
        /// </summary>
        public WebRuntime(
            Configs configs,
            IUmbracoVersion umbracoVersion,
            IIOHelper ioHelper,
            ILogger logger,
            IProfiler profiler,
            IHostingEnvironment hostingEnvironment,
            IBackOfficeInfo backOfficeInfo,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            IMainDom mainDom,
            ITypeFinder typeFinder,
            IRequestCache requestCache,
            IUmbracoBootPermissionChecker umbracoBootPermissionChecker):
            base(configs, umbracoVersion, ioHelper, logger, profiler ,umbracoBootPermissionChecker, hostingEnvironment, backOfficeInfo, dbProviderFactoryCreator, mainDom, typeFinder)
        {
            _requestCache = requestCache;
        }

        /// <inheritdoc/>
        public override IFactory Configure(IRegister register)
        {

            var profilingLogger =  new ProfilingLogger(Logger, Profiler);
            var umbracoVersion = new UmbracoVersion();
            using (var timer = profilingLogger.TraceDuration<CoreRuntime>(
                $"Booting Umbraco {umbracoVersion.SemanticVersion.ToSemanticString()}.",
                "Booted.",
                "Boot failed."))
            {
                Logger.Info<CoreRuntime>("Booting site '{HostingSiteName}', app '{HostingApplicationId}', path '{HostingPhysicalPath}', server '{MachineName}'.",
                    HostingEnvironment.SiteName,
                    HostingEnvironment.ApplicationId,
                    HostingEnvironment.ApplicationPhysicalPath,
                    NetworkHelper.MachineName);
                Logger.Debug<CoreRuntime>("Runtime: {Runtime}", GetType().FullName);

                var factory =  base.Configure(register);

                // now (and only now) is the time to switch over to perWebRequest scopes.
                // up until that point we may not have a request, and scoped services would
                // fail to resolve - but we run Initialize within a factory scope - and then,
                // here, we switch the factory to bind scopes to requests
                factory.EnablePerWebRequestScope();

                return factory;
            }


        }

        #region Getters

        protected override AppCaches GetAppCaches() => new AppCaches(
                // we need to have the dep clone runtime cache provider to ensure
                // all entities are cached properly (cloned in and cloned out)
                new DeepCloneAppCache(new ObjectCacheAppCache()),
                // we need request based cache when running in web-based context
                _requestCache,
                new IsolatedCaches(type =>
                    // we need to have the dep clone runtime cache provider to ensure
                    // all entities are cached properly (cloned in and cloned out)
                    new DeepCloneAppCache(new ObjectCacheAppCache())));

        #endregion
    }
}

