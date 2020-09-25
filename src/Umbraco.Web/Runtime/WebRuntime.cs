using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
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

        [Obsolete("Use the ctor with all parameters instead")]
        public WebRuntime(UmbracoApplicationBase umbracoApplication)
            : this(umbracoApplication, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRuntime"/> class.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        public WebRuntime(UmbracoApplicationBase umbracoApplication, ILogger logger, IMainDom mainDom)
            : base(logger, mainDom)
        {
            _umbracoApplication = umbracoApplication;
        }

        /// <inheritdoc/>
        public override IFactory Boot(IRegister register)
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

            var factory = base.Boot(register);

            // factory can be null if part of the boot process fails
            if (factory != null)
            {
                // now (and only now) is the time to switch over to perWebRequest scopes.
                // up until that point we may not have a request, and scoped services would
                // fail to resolve - but we run Initialize within a factory scope - and then,
                // here, we switch the factory to bind scopes to requests
                factory.EnablePerWebRequestScope();
            }
            
            return factory;
        }

        #region Getters

        protected override IProfiler GetProfiler() => _webProfiler;

        protected override AppCaches GetAppCaches() => new AppCaches(
                // we need to have the dep clone runtime cache provider to ensure
                // all entities are cached properly (cloned in and cloned out)
                new DeepCloneAppCache(new WebCachingAppCache(HttpRuntime.Cache)),
                // we need request based cache when running in web-based context
                new HttpRequestAppCache(),
                new IsolatedCaches(type =>
                    // we need to have the dep clone runtime cache provider to ensure
                    // all entities are cached properly (cloned in and cloned out)
                    new DeepCloneAppCache(new ObjectCacheAppCache())));

        #endregion
    }
}

