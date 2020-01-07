using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Web.Cache;
using Umbraco.Web.Composing;
using Umbraco.Web.Logging;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Runtime
{
    /// <summary>
    /// Represents the Web Umbraco runtime.
    /// </summary>
    /// <remarks>On top of CoreRuntime, handles all of the web-related runtime aspects of Umbraco.</remarks>
    public class WebRuntime : CoreRuntime
    {
        private readonly UmbracoApplicationBase _umbracoApplication;
        private BuildManagerTypeFinder _typeFinder;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRuntime"/> class.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        public WebRuntime(
            UmbracoApplicationBase umbracoApplication,
            Configs configs,
            IUmbracoVersion umbracoVersion,
            IIOHelper ioHelper,
            ILogger logger,
            IProfiler profiler,
            IHostingEnvironment hostingEnvironment,
            IBackOfficeInfo backOfficeInfo,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            IBulkSqlInsertProvider bulkSqlInsertProvider,
            IMainDom mainDom):
            base(configs, umbracoVersion, ioHelper, logger, profiler ,new AspNetUmbracoBootPermissionChecker(), hostingEnvironment, backOfficeInfo, dbProviderFactoryCreator, bulkSqlInsertProvider, mainDom)
        {
            _umbracoApplication = umbracoApplication;

            Profiler = GetWebProfiler();
        }

        private IProfiler GetWebProfiler()
        {
            // create and start asap to profile boot
            if (!State.Debug)
            {
                // should let it be null, that's how MiniProfiler is meant to work,
                // but our own IProfiler expects an instance so let's get one
                return new VoidProfiler();
            }

            var webProfiler = new WebProfiler();
            webProfiler.Start();

            return webProfiler;
        }

        /// <inheritdoc/>
        public override IFactory Boot(IRegister register)
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

                var factory = Current.Factory = base.Boot(register);

                // now (and only now) is the time to switch over to perWebRequest scopes.
                // up until that point we may not have a request, and scoped services would
                // fail to resolve - but we run Initialize within a factory scope - and then,
                // here, we switch the factory to bind scopes to requests
                factory.EnablePerWebRequestScope();

                return factory;
            }


        }

        #region Getters

        protected override ITypeFinder GetTypeFinder() => _typeFinder ?? (_typeFinder = new BuildManagerTypeFinder(IOHelper, HostingEnvironment, Logger, new BuildManagerTypeFinder.TypeFinderConfig()));

        protected override AppCaches GetAppCaches() => new AppCaches(
                // we need to have the dep clone runtime cache provider to ensure
                // all entities are cached properly (cloned in and cloned out)
                new DeepCloneAppCache(new WebCachingAppCache(HttpRuntime.Cache, TypeFinder)),
                // we need request based cache when running in web-based context
                new HttpRequestAppCache(() => HttpContext.Current?.Items, TypeFinder),
                new IsolatedCaches(type =>
                    // we need to have the dep clone runtime cache provider to ensure
                    // all entities are cached properly (cloned in and cloned out)
                    new DeepCloneAppCache(new ObjectCacheAppCache(TypeFinder))));

        #endregion
    }
}

