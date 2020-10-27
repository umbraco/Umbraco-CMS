using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Templates;
using Umbraco.Core.Runtime;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Composing.CompositionExtensions;
using Umbraco.Web.Macros;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Security;
using Umbraco.Web.Security.Providers;

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

            composition.Services.AddTransient<UmbracoInjectedModule>();


            // register membership stuff
            composition.Register(factory => MembershipProviderExtensions.GetMembersMembershipProvider());
            composition.Register(factory => Roles.Enabled ? Roles.Provider : new MembersRoleProvider(factory.GetRequiredService<IMemberService>()));
            composition.Services.AddScoped<MembershipHelper>();
            composition.Register<IPublishedMemberCache>(factory => factory.GetRequiredService<IUmbracoContext>().PublishedSnapshot.Members);
            composition.RegisterUnique<IMemberUserKeyProvider, MemberUserKeyProvider>();
            composition.RegisterUnique<IPublicAccessChecker, PublicAccessChecker>();


            // register the umbraco helper - this is Transient! very important!
            // also, if not level.Run, we cannot really use the helper (during upgrade...)
            // so inject a "void" helper (not exactly pretty but...)
            if (composition.RuntimeState.Level == RuntimeLevel.Run)
                composition.Register<UmbracoHelper>(factory =>
                {
                    var umbCtx = factory.GetRequiredService<IUmbracoContext>();
                    return new UmbracoHelper(umbCtx.IsFrontEndUmbracoRequest ? umbCtx.PublishedRequest?.PublishedContent : null, factory.GetRequiredService<ICultureDictionaryFactory>(),
                        factory.GetRequiredService<IUmbracoComponentRenderer>(), factory.GetRequiredService<IPublishedContentQuery>());
                });
            else
                composition.Register(_ => new UmbracoHelper());

            composition.RegisterUnique<RoutableDocumentFilter>();

            // configure the container for web
            //composition.ConfigureForWeb();

            //composition
            /* TODO: This will depend on if we use ServiceBasedControllerActivator - see notes in Startup.cs
             * You will likely need to set DefaultRenderMvcControllerType on Umbraco.Web.Composing.Current
             * which is what the extension method below did previously.
             */
            //.ComposeUmbracoControllers(GetType().Assembly)
            //.SetDefaultRenderMvcController</*RenderMvcController*/ Controller>(); // default controller for template views

            //we need to eagerly scan controller types since they will need to be routed
            composition.WithCollectionBuilder<SurfaceControllerTypeCollectionBuilder>()
                .Add(composition.TypeLoader.GetSurfaceControllers());


            // auto-register views
            //composition.RegisterAuto(typeof(UmbracoViewPage<>));
        }
    }
}

