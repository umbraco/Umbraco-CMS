using System.Web;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Runtime;
using Umbraco.Web.Logging;

namespace Umbraco.Web.Runtime
{
    /// <summary>
    /// Represents the Web Umbraco runtime.
    /// </summary>
    /// <remarks>On top of CoreRuntime, handles all of the web-related runtime aspects of Umbraco.</remarks>
    public class WebRuntime : CoreRuntime
    {
        private readonly UmbracoApplicationBase _umbracoApplication;
        private IProfiler _webProfiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRuntime"/> class.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        public WebRuntime(UmbracoApplicationBase umbracoApplication)
            : base()
        {
            _umbracoApplication = umbracoApplication;
        }

        /// <inheritdoc/>
        public override void Boot(IContainer container)
        {
            // create and start asap to profile boot
            var debug = GlobalSettings.DebugMode;
            if (debug)
            {
                _webProfiler = new WebProfiler();
                _webProfiler.Start();
            }
            else
            {
                // should let it be null, that's how MiniProfiler is meant to work,
                // but our own IProfiler expects an instance so let's get one
                _webProfiler = new VoidProfiler();
            }

            base.Boot(container);

            // now (and only now) is the time to switch over to perWebRequest scopes
            container.EnablePerWebRequestScope();
        }

        /// <inheritdoc/>
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            var container = composition.Container;

            // some components may want to initialize with the UmbracoApplicationBase
            // well, they should not - we should not do this
            // TODO remove this eventually.
            container.RegisterInstance(_umbracoApplication);
        }

        #region Getters

        protected override IProfiler GetProfiler() => new WebProfiler();

        protected override CacheHelper GetAppCaches() => new CacheHelper(
                // we need to have the dep clone runtime cache provider to ensure
                // all entities are cached properly (cloned in and cloned out)
                new DeepCloneRuntimeCacheProvider(new HttpRuntimeCacheProvider(HttpRuntime.Cache)),
                new StaticCacheProvider(),
                // we need request based cache when running in web-based context
                new HttpRequestCacheProvider(),
                new IsolatedRuntimeCache(type =>
                    // we need to have the dep clone runtime cache provider to ensure
                    // all entities are cached properly (cloned in and cloned out)
                    new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider())));

        #endregion
    }
}

