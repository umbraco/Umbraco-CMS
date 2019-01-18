﻿using System.Web;
using System.Web.Security;
using Examine;
using Microsoft.AspNet.SignalR;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Runtime;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Cache;
using Umbraco.Web.Composing.Composers;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Dictionary;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Models.PublishedContent;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;
using Umbraco.Web.Security;
using Umbraco.Web.Security.Providers;
using Umbraco.Web.Services;
using Umbraco.Web.SignalR;
using Umbraco.Web.Tour;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Runtime
{
    [ComposeAfter(typeof(CoreRuntimeComposer))]
    public sealed class WebRuntimeComposer : ComponentComposer<WebRuntimeComponent>, IRuntimeComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Register<UmbracoInjectedModule>();

            composition.RegisterUnique<IHttpContextAccessor, AspNetHttpContextAccessor>(); // required for hybrid accessors

            composition.ComposeWebMappingProfiles();

            //register the install components
            //NOTE: i tried to not have these registered if we weren't installing or upgrading but post install when the site restarts
            //it still needs to use the install controller so we can't do that
            composition.ComposeInstaller();

            // register membership stuff
            composition.Register(factory => Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider());
            composition.Register(factory => Roles.Enabled ? Roles.Provider : new MembersRoleProvider(factory.GetInstance<IMemberService>()));
            composition.Register<MembershipHelper>();

            // register accessors for cultures
            composition.RegisterUnique<IDefaultCultureAccessor, DefaultCultureAccessor>();
            composition.RegisterUnique<IVariationContextAccessor, HttpContextVariationContextAccessor>();

            // register the http context and umbraco context accessors
            // we *should* use the HttpContextUmbracoContextAccessor, however there are cases when
            // we have no http context, eg when booting Umbraco or in background threads, so instead
            // let's use an hybrid accessor that can fall back to a ThreadStatic context.
            composition.RegisterUnique<IUmbracoContextAccessor, HybridUmbracoContextAccessor>();

            // register a per-request HttpContextBase object
            // is per-request so only one wrapper is created per request
            composition.Register<HttpContextBase>(factory => new HttpContextWrapper(factory.GetInstance<IHttpContextAccessor>().HttpContext), Lifetime.Request);

            // register the published snapshot accessor - the "current" published snapshot is in the umbraco context
            composition.RegisterUnique<IPublishedSnapshotAccessor, UmbracoContextPublishedSnapshotAccessor>();

            // we should stop injecting UmbracoContext and always inject IUmbracoContextAccessor, however at the moment
            // there are tons of places (controllers...) which require UmbracoContext in their ctor - so let's register
            // a way to inject the UmbracoContext - and register it per-request to be more efficient
            //TODO: stop doing this
            composition.Register(factory => factory.GetInstance<IUmbracoContextAccessor>().UmbracoContext, Lifetime.Request);

            // register the umbraco helper
            composition.RegisterUnique<UmbracoHelper>();

            // register distributed cache
            composition.RegisterUnique(f => new DistributedCache());

            // replace some services
            composition.RegisterUnique<IEventMessagesFactory, DefaultEventMessagesFactory>();
            composition.RegisterUnique<IEventMessagesAccessor, HybridEventMessagesAccessor>();
            composition.RegisterUnique<IApplicationTreeService, ApplicationTreeService>();
            composition.RegisterUnique<ISectionService, SectionService>();

            composition.RegisterUnique<IExamineManager>(factory => ExamineManager.Instance);

            // configure the container for web
            composition.ConfigureForWeb();


            composition.RegisterUnique<Dashboards>();

            composition
                .ComposeUmbracoControllers(GetType().Assembly)
                .SetDefaultRenderMvcController<RenderMvcController>(); // default controller for template views

            composition.WithCollectionBuilder<SearchableTreeCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetTypes<ISearchableTree>()); // fixme which searchable trees?!

            composition.Register<UmbracoTreeSearcher>(Lifetime.Request);

            composition.WithCollectionBuilder<EditorValidatorCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetTypes<IEditorValidator>());

            composition.WithCollectionBuilder<TourFilterCollectionBuilder>();

            composition.RegisterUnique<UmbracoFeatures>();

            composition.WithCollectionBuilder<ActionCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetTypes<IAction>());

            var surfaceControllerTypes = new SurfaceControllerTypeCollection(composition.TypeLoader.GetSurfaceControllers());
            composition.RegisterUnique(surfaceControllerTypes);

            var umbracoApiControllerTypes = new UmbracoApiControllerTypeCollection(composition.TypeLoader.GetUmbracoApiControllers());
            composition.RegisterUnique(umbracoApiControllerTypes);

            // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
            // discovered when CoreBootManager configures the converters. We HAVE to remove one of them
            // here because there cannot be two converters for one property editor - and we want the full
            // RteMacroRenderingValueConverter that converts macros, etc. So remove TinyMceValueConverter.
            // (the limited one, defined in Core, is there for tests) - same for others
            composition.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Remove<TinyMceValueConverter>()
                .Remove<TextStringValueConverter>()
                .Remove<MarkdownEditorValueConverter>();

            // add all known factories, devs can then modify this list on application
            // startup either by binding to events or in their own global.asax
            composition.WithCollectionBuilder<FilteredControllerFactoryCollectionBuilder>()
                .Append<RenderControllerFactory>();

            composition.WithCollectionBuilder<UrlProviderCollectionBuilder>()
                .Append<AliasUrlProvider>()
                .Append<DefaultUrlProvider>()
                .Append<CustomRouteUrlProvider>();

            composition.RegisterUnique<IContentLastChanceFinder, ContentFinderByLegacy404>();

            composition.WithCollectionBuilder<ContentFinderCollectionBuilder>()
                // all built-in finders in the correct order,
                // devs can then modify this list on application startup
                .Append<ContentFinderByPageIdQuery>()
                .Append<ContentFinderByUrl>()
                .Append<ContentFinderByIdPath>()
                //.Append<ContentFinderByUrlAndTemplate>() // disabled, this is an odd finder
                .Append<ContentFinderByUrlAlias>()
                .Append<ContentFinderByRedirectUrl>();

            composition.RegisterUnique<ISiteDomainHelper, SiteDomainHelper>();

            composition.RegisterUnique<ICultureDictionaryFactory, DefaultCultureDictionaryFactory>();

            // register *all* checks, except those marked [HideFromTypeFinder] of course
            composition.WithCollectionBuilder<HealthCheckCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetTypes<HealthCheck.HealthCheck>());

            composition.WithCollectionBuilder<HealthCheckNotificationMethodCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetTypes<HealthCheck.NotificationMethods.IHealthCheckNotificationMethod>());

            // auto-register views
            composition.RegisterAuto(typeof(UmbracoViewPage<>));

            // register published router
            composition.RegisterUnique<PublishedRouter>();
            composition.Register(_ => Current.Configs.Settings().WebRouting);

            // register preview SignalR hub
            composition.RegisterUnique(_ => GlobalHost.ConnectionManager.GetHubContext<PreviewHub>());

            // register properties fallback
            composition.RegisterUnique<IPublishedValueFallback, PublishedValueFallback>();

            // register known content apps
            composition.WithCollectionBuilder<ContentAppFactoryCollectionBuilder>()
                .Append<ListViewContentAppFactory>()
                .Append<ContentEditorContentAppFactory>()
                .Append<ContentInfoContentAppFactory>();
        }
    }
}
