using System;
using System.Web;
using System.Web.Http;
using Examine;
using LightInject;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.Persistence;
using Umbraco.Core.Plugins;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using Umbraco.Web.DependencyInjection;
using Umbraco.Web.Dictionary;
using Umbraco.Web.Editors;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.DataIntegrity;
using Umbraco.Web.Media;
using Umbraco.Web.Media.ThumbnailProviders;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Services;
using Umbraco.Web.WebApi;
using Umbraco.Web._Legacy.Actions;
using UmbracoExamine;
using Action = System.Action;

namespace Umbraco.Web
{
    [RequireComponent(typeof(CoreRuntimeComponent))]
    public class WebRuntimeComponent : UmbracoComponentBase, IRuntimeComponent
    {
        public override void Compose(ServiceContainer container)
        {
            base.Compose(container);

            container.RegisterFrom<WebModelMappersCompositionRoot>();

            var pluginManager = container.GetInstance<PluginManager>();
            var logger = container.GetInstance<ILogger>();

            // register the http context and umbraco context accessors
            // we *should* use the HttpContextUmbracoContextAccessor, however there are cases when
            // we have no http context, eg when booting Umbraco or in background threads, so instead
            // let's use an hybrid accessor that can fall back to a ThreadStatic context.
            container.RegisterSingleton<IHttpContextAccessor, AspNetHttpContextAccessor>(); // replaces HttpContext.Current
            container.RegisterSingleton<IUmbracoContextAccessor, HybridUmbracoContextAccessor>();

            // register a per-request HttpContextBase object
            // is per-request so only one wrapper is created per request
            container.Register<HttpContextBase>(factory => new HttpContextWrapper(factory.GetInstance<IHttpContextAccessor>().HttpContext), new PerRequestLifeTime());

            // register the facade accessor - the "current" facade is in the umbraco context
            container.RegisterSingleton<IFacadeAccessor, UmbracoContextFacadeAccessor>();

            // register the umbraco database accessor
            // have to use the hybrid thing...
            container.RegisterSingleton<IUmbracoDatabaseAccessor, HybridUmbracoDatabaseAccessor>();

            // register a per-request UmbracoContext object
            // no real need to be per request but assuming it is faster
            container.Register(factory => factory.GetInstance<IUmbracoContextAccessor>().UmbracoContext, new PerRequestLifeTime());

            // register the umbraco helper
            container.RegisterSingleton<UmbracoHelper>();

            // replace some services
            container.RegisterSingleton<IEventMessagesFactory, DefaultEventMessagesFactory>();
            container.RegisterSingleton<IEventMessagesAccessor, HybridEventMessagesAccessor>();
            container.RegisterSingleton<IApplicationTreeService, ApplicationTreeService>();
            container.RegisterSingleton<ISectionService, SectionService>();

            container.RegisterSingleton<IExamineIndexCollectionAccessor, ExamineIndexCollectionAccessor>();

            // fixme - still, doing it before we get the INITIALIZE scope is a BAD idea
            // fixme - also note that whatever you REGISTER in a scope, stays registered => create the scope beforehand?
            // fixme - BUT not enough, once per-request is enabled it WANTS per-request scope even though a scope already exists
            // IoC setup for LightInject for MVC/WebApi
            // see comments on MixedScopeManagerProvider for explainations of what we are doing here
            var smp = container.ScopeManagerProvider as MixedScopeManagerProvider;
            if (smp == null) throw new Exception("Container.ScopeManagerProvider is not MixedScopeManagerProvider.");
            container.EnableMvc(); // does container.EnablePerWebRequestScope()
            container.ScopeManagerProvider = smp; // reverts - we will do it last (in WebRuntime)

            container.RegisterMvcControllers(pluginManager, GetType().Assembly);
            container.EnableWebApi(GlobalConfiguration.Configuration);
            container.RegisterApiControllers(pluginManager, GetType().Assembly);

            XsltExtensionCollectionBuilder.Register(container)
                .AddExtensionObjectProducer(() => pluginManager.ResolveXsltExtensions());

            EditorValidatorCollectionBuilder.Register(container)
                .AddProducer(() => pluginManager.ResolveTypes<IEditorValidator>());

            // set the default RenderMvcController
            Current.DefaultRenderMvcControllerType = typeof(RenderMvcController); // fixme WRONG!

            //Override the default server messenger, we need to check if the legacy dist calls is enabled, if that is the
            // case, then we'll set the default messenger to be the old one, otherwise we'll set it to the db messenger
            // which will always be on.
            if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled)
            {
                //set the legacy one by default - this maintains backwards compat
                container.Register<IServerMessenger>(factory => new BatchedWebServiceServerMessenger(() =>
                {
                    var applicationContext = factory.GetInstance<ApplicationContext>();
                    //we should not proceed to change this if the app/database is not configured since there will
                    // be no user, plus we don't need to have server messages sent if this is the case.
                    if (applicationContext.IsConfigured && applicationContext.DatabaseContext.IsDatabaseConfigured)
                    {
                        //disable if they are not enabled
                        if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled == false)
                        {
                            return null;
                        }

                        try
                        {
                            var user = applicationContext.Services.UserService.GetUserById(UmbracoConfig.For.UmbracoSettings().DistributedCall.UserId);
                            return new Tuple<string, string>(user.Username, user.RawPasswordValue);
                        }
                        catch (Exception e)
                        {
                            logger.Error<WebRuntime>("An error occurred trying to set the IServerMessenger during application startup", e);
                            return null;
                        }
                    }
                    logger.Warn<WebRuntime>("Could not initialize the DefaultServerMessenger, the application is not configured or the database is not configured");
                    return null;
                }), new PerContainerLifetime());
            }
            else
            {
                container.Register<IServerMessenger>(factory => new BatchedDatabaseServerMessenger(
                    factory.GetInstance<ApplicationContext>(),
                    true,
                    //Default options for web including the required callbacks to build caches
                    new DatabaseServerMessengerOptions
                    {
                        //These callbacks will be executed if the server has not been synced
                        // (i.e. it is a new server or the lastsynced.txt file has been removed)
                        InitializingCallbacks = new Action[]
                        {
                            //rebuild the xml cache file if the server is not synced
                            () =>
                            {
                                // rebuild the facade caches entirely, if the server is not synced
                                // this is equivalent to DistributedCache RefreshAllFacade but local only
                                // (we really should have a way to reuse RefreshAllFacade... locally)
                                // note: refresh all content & media caches does refresh content types too
    					        var svc = Current.FacadeService;
                                bool ignored1, ignored2;
                                svc.Notify(new[] { new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll) });
                                svc.Notify(new[] { new ContentCacheRefresher.JsonPayload(0, TreeChangeTypes.RefreshAll) }, out ignored1, out ignored2);
                                svc.Notify(new[] { new MediaCacheRefresher.JsonPayload(0, TreeChangeTypes.RefreshAll) }, out ignored1);
                            },
                            //rebuild indexes if the server is not synced
                            // NOTE: This will rebuild ALL indexes including the members, if developers want to target specific
                            // indexes then they can adjust this logic themselves.
                            () => RebuildIndexes(false)
                        }
                    }), new PerContainerLifetime());
            }

            ActionCollectionBuilder.Register(container)
                .SetProducer(() => pluginManager.ResolveActions());

            var surfaceControllerTypes = new SurfaceControllerTypeCollection(pluginManager.ResolveSurfaceControllers());
            container.RegisterInstance(surfaceControllerTypes);

            var umbracoApiControllerTypes = new UmbracoApiControllerTypeCollection(pluginManager.ResolveUmbracoApiControllers());
            container.RegisterInstance(umbracoApiControllerTypes);

            // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
            // discovered when CoreBootManager configures the converters. We HAVE to remove one of them
            // here because there cannot be two converters for one property editor - and we want the full
            // RteMacroRenderingValueConverter that converts macros, etc. So remove TinyMceValueConverter.
            // (the limited one, defined in Core, is there for tests) - same for others
            container.GetInstance<PropertyValueConverterCollectionBuilder>()
                .Remove<TinyMceValueConverter>()
                .Remove<TextStringValueConverter>()
                .Remove<MarkdownEditorValueConverter>()
                .Remove<ImageCropperValueConverter>();

            // add all known factories, devs can then modify this list on application
            // startup either by binding to events or in their own global.asax
            FilteredControllerFactoryCollectionBuilder.Register(container)
                .Append<RenderControllerFactory>();

            UrlProviderCollectionBuilder.Register(container)
                //.Append<AliasUrlProvider>() // not enabled by default
                .Append<DefaultUrlProvider>()
                .Append<CustomRouteUrlProvider>();

            container.Register<IContentLastChanceFinder, ContentFinderByLegacy404>();

            ContentFinderCollectionBuilder.Register(container)
                // all built-in finders in the correct order,
                // devs can then modify this list on application startup
                .Append<ContentFinderByPageIdQuery>()
                .Append<ContentFinderByNiceUrl>()
                .Append<ContentFinderByIdPath>()
                .Append<ContentFinderByNiceUrlAndTemplate>()
                .Append<ContentFinderByProfile>()
                .Append<ContentFinderByUrlAlias>()
                .Append<ContentFinderByRedirectUrl>();

            container.Register<ISiteDomainHelper, SiteDomainHelper>();

            ThumbnailProviderCollectionBuilder.Register(container)
                .Add(pluginManager.ResolveThumbnailProviders());

            ImageUrlProviderCollectionBuilder.Register(container)
                .Append(pluginManager.ResolveImageUrlProviders());

            container.RegisterSingleton<ICultureDictionaryFactory, DefaultCultureDictionaryFactory>();

            HealthCheckCollectionBuilder.Register(container)
                .AddProducer(() => pluginManager.ResolveTypes<HealthCheck.HealthCheck>())
                .Exclude<XmlDataIntegrityHealthCheck>(); // fixme must remove else NuCache dies!
            // but we should also have one for NuCache AND NuCache should be a component that does all this
        }

        protected virtual void RebuildIndexes(bool onlyEmptyIndexes)
        {
            if (Current.ApplicationContext.IsConfigured == false || Current.ApplicationContext.DatabaseContext.IsDatabaseConfigured == false)
            {
                return;
            }

            foreach (var indexer in ExamineManager.Instance.IndexProviders)
            {
                if (onlyEmptyIndexes == false || indexer.Value.IsIndexNew())
                {
                    indexer.Value.RebuildIndex();
                }
            }
        }
    }
}
