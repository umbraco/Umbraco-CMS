using System.Linq;
using System.Web.Security;
using Examine;
using Microsoft.AspNet.SignalR;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Cookie;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Core.Install;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Runtime;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Web.Actions;
using Umbraco.Web.Cache;
using Umbraco.Web.Composing.CompositionExtensions;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Hosting;
using Umbraco.Web.Install;
using Umbraco.Web.Macros;
using Umbraco.Web.Media.EmbedProviders;
using Umbraco.Web.Models.PublishedContent;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;
using Umbraco.Web.Sections;
using Umbraco.Web.Security;
using Umbraco.Web.Security.Providers;
using Umbraco.Web.Services;
using Umbraco.Web.SignalR;
using Umbraco.Web.Templates;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using Umbraco.Web.PropertyEditors;
using Umbraco.Examine;
using Umbraco.Net;
using Umbraco.Core.Request;
using Umbraco.Core.Session;
using Umbraco.Web.AspNet;
using Umbraco.Core.Media;
using Umbraco.Infrastructure.Media;

namespace Umbraco.Web.Runtime
{
    // web's initial composer composes after core's, and before all core composers
    [ComposeAfter(typeof(CoreInitialComposer))]
    [ComposeBefore(typeof(ICoreComposer))]
    public sealed class WebInitialComposer : ComponentComposer<WebInitialComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Register<UmbracoInjectedModule>();
            composition.Register<IIpResolver, AspNetIpResolver>();

            composition.Register<IUserAgentProvider, AspNetUserAgentProvider>();
            composition.Register<AspNetSessionManager>(Lifetime.Singleton);
            composition.Register<ISessionIdResolver>(factory => factory.GetInstance<AspNetSessionManager>(), Lifetime.Singleton);
            composition.Register<ISessionManager>(factory => factory.GetInstance<AspNetSessionManager>(), Lifetime.Singleton);

            composition.Register<IRequestAccessor, AspNetRequestAccessor>(Lifetime.Singleton);

            composition.Register<IUmbracoApplicationLifetime, AspNetUmbracoApplicationLifetime>(Lifetime.Singleton);
            composition.Register<IPasswordHasher, AspNetPasswordHasher>();
            composition.Register<IFilePermissionHelper, FilePermissionHelper>(Lifetime.Singleton);

            composition.RegisterUnique<IHttpContextAccessor, AspNetHttpContextAccessor>(); // required for hybrid accessors
            composition.RegisterUnique<ICookieManager, AspNetCookieManager>();


            composition.ComposeWebMappingProfiles();

            //register the install components
            //NOTE: i tried to not have these registered if we weren't installing or upgrading but post install when the site restarts
            //it still needs to use the install controller so we can't do that
            composition.ComposeInstaller();

            // register membership stuff
            composition.Register(factory => MembershipProviderExtensions.GetMembersMembershipProvider());
            composition.Register(factory => Roles.Enabled ? Roles.Provider : new MembersRoleProvider(factory.GetInstance<IMemberService>()));
            composition.Register<MembershipHelper>(Lifetime.Request);
            composition.Register<IPublishedMemberCache>(factory => factory.GetInstance<IUmbracoContext>().PublishedSnapshot.Members);
            composition.RegisterUnique<IMemberUserKeyProvider, MemberUserKeyProvider>();
            composition.RegisterUnique<IPublicAccessChecker, PublicAccessChecker>();

            // register the umbraco context factory
            composition.RegisterUnique<IUmbracoContextFactory, UmbracoContextFactory>();

            composition.RegisterUnique<ITemplateRenderer, TemplateRenderer>();
            composition.RegisterUnique<IMacroRenderer, MacroRenderer>();

            composition.RegisterUnique<IUmbracoComponentRenderer, UmbracoComponentRenderer>();

            // register the umbraco helper - this is Transient! very important!
            // also, if not level.Run, we cannot really use the helper (during upgrade...)
            // so inject a "void" helper (not exactly pretty but...)
            if (composition.RuntimeState.Level == RuntimeLevel.Run)
                composition.Register<UmbracoHelper>(factory =>
                {
                    var umbCtx = factory.GetInstance<IUmbracoContext>();
                    return new UmbracoHelper(umbCtx.IsFrontEndUmbracoRequest ? umbCtx.PublishedRequest?.PublishedContent : null, factory.GetInstance<ICultureDictionaryFactory>(),
                        factory.GetInstance<IUmbracoComponentRenderer>(), factory.GetInstance<IPublishedContentQuery>());
                });
            else
                composition.Register(_ => new UmbracoHelper());

            composition.RegisterUnique<RoutableDocumentFilter>();

            // configure the container for web
            composition.ConfigureForWeb();

            composition
                .ComposeUmbracoControllers(GetType().Assembly)
                .SetDefaultRenderMvcController<RenderMvcController>(); // default controller for template views

            //we need to eagerly scan controller types since they will need to be routed
            composition.WithCollectionBuilder<SurfaceControllerTypeCollectionBuilder>()
                .Add(composition.TypeLoader.GetSurfaceControllers());
            var umbracoApiControllerTypes = composition.TypeLoader.GetUmbracoApiControllers().ToList();
            composition.WithCollectionBuilder<UmbracoApiControllerTypeCollectionBuilder>()
                .Add(umbracoApiControllerTypes);


            // add all known factories, devs can then modify this list on application
            // startup either by binding to events or in their own global.asax
            composition.FilteredControllerFactory()
                .Append<RenderControllerFactory>();

            // auto-register views
            composition.RegisterAuto(typeof(UmbracoViewPage<>));

            // register preview SignalR hub
            composition.RegisterUnique(_ => GlobalHost.ConnectionManager.GetHubContext<PreviewHub>());

            // register back office trees
            // the collection builder only accepts types inheriting from TreeControllerBase
            // and will filter out those that are not attributed with TreeAttribute
            composition.Trees()
                .AddTreeControllers(umbracoApiControllerTypes.Where(x => typeof(TreeControllerBase).IsAssignableFrom(x)));


            // Config manipulator
            composition.RegisterUnique<IConfigManipulator, XmlConfigManipulator>();

            //ApplicationShutdownRegistry
            composition.RegisterUnique<IApplicationShutdownRegistry, AspNetApplicationShutdownRegistry>();


        }
    }
}

