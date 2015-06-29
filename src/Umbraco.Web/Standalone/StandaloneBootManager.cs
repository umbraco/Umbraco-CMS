using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using umbraco.interfaces;

namespace Umbraco.Web.Standalone
{
    /// <summary>
    /// A boot manager for use in standalone applications.
    /// </summary>
    internal class StandaloneBootManager : CoreBootManager
    {
        // TODO
        // this is highly experimental and probably not complete - not for production usage!
        // also, could we inherit from WebBootManager?

        private readonly IEnumerable<Type> _handlersToAdd;
        private readonly IEnumerable<Type> _handlersToRemove;
        private readonly string _baseDirectory;

        public StandaloneBootManager(UmbracoApplicationBase umbracoApplication, IEnumerable<Type> handlersToAdd, IEnumerable<Type> handlersToRemove, string baseDirectory)
            : base(umbracoApplication)
        {
            _handlersToAdd = handlersToAdd;
            _handlersToRemove = handlersToRemove;
            _baseDirectory = baseDirectory;

            base.InitializeApplicationRootPath(_baseDirectory);

            // this is only here to ensure references to the assemblies needed for
            // the DataTypesResolver otherwise they won't be loaded into the AppDomain.
            var interfacesAssemblyName = typeof(IDataType).Assembly.FullName;

            // TODO there's also that one... but we don't use it in standalone?
            //using umbraco.editorControls;
            //var editorControlsAssemblyName = typeof(uploadField).Assembly.FullName;
        }

        protected override void InitializeApplicationEventsResolver()
        {
            base.InitializeApplicationEventsResolver();
            foreach (var type in _handlersToAdd)
                ApplicationEventsResolver.Current.AddType(type);
            foreach (var type in _handlersToRemove)
                ApplicationEventsResolver.Current.RemoveType(type);
        }

        protected override void InitializeResolvers()
        {
            base.InitializeResolvers();

            var caches = new PublishedCaches(
                new PublishedCache.XmlPublishedCache.PublishedContentCache(),
                new PublishedCache.XmlPublishedCache.PublishedMediaCache(ApplicationContext.Current));

            PublishedCachesResolver.Current = new PublishedCachesResolver(caches);

            UrlProviderResolver.Current = new UrlProviderResolver(ServiceProvider, LoggerResolver.Current.Logger, typeof(DefaultUrlProvider));
            SiteDomainHelperResolver.Current = new SiteDomainHelperResolver(new SiteDomainHelper());

            ContentLastChanceFinderResolver.Current = new ContentLastChanceFinderResolver();
			ContentFinderResolver.Current = new ContentFinderResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
				typeof (ContentFinderByPageIdQuery),
				typeof (ContentFinderByNiceUrl),
				typeof (ContentFinderByIdPath),
                typeof (ContentFinderByNotFoundHandlers)
            );

            // TODO what else?
        }

        // can't create context before resolution is frozen!
        protected override void FreezeResolution()
        {
            base.FreezeResolution();

            var httpContext = new StandaloneHttpContext();
            UmbracoContext.EnsureContext(httpContext, ApplicationContext.Current, new WebSecurity(httpContext, ApplicationContext.Current), false, false);
        }
    }
}
