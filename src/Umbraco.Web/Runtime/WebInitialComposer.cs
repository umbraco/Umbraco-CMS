using System.Web.Security;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Templates;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Macros;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Security;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Web.Runtime
{
    [ComposeBefore(typeof(ICoreComposer))]
    public sealed class WebInitialComposer : ComponentComposer<WebInitialComponent>
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);

            builder.Services.AddTransient<UmbracoInjectedModule>();


            // register membership stuff
            builder.Services.AddTransient(factory => MembershipProviderExtensions.GetMembersMembershipProvider());
            builder.Services.AddTransient(factory => Roles.Enabled ? Roles.Provider : new MembersRoleProvider(factory.GetRequiredService<IMemberService>()));
            builder.Services.AddScoped<MembershipHelper>();
            builder.Services.AddTransient<IPublishedMemberCache>(factory => factory.GetRequiredService<IUmbracoContext>().PublishedSnapshot.Members);
            builder.Services.AddUnique<IMemberUserKeyProvider, MemberUserKeyProvider>();
            builder.Services.AddUnique<IPublicAccessChecker, PublicAccessChecker>();

            builder.Services.AddTransient<UmbracoHelper>(factory =>
            {
                var state = factory.GetRequiredService<IRuntimeState>();

                if (state.Level == RuntimeLevel.Run)
                {
                    var umbCtx = factory.GetRequiredService<IUmbracoContext>();
                    return new UmbracoHelper(umbCtx.IsFrontEndUmbracoRequest() ? umbCtx.PublishedRequest?.PublishedContent : null, factory.GetRequiredService<ICultureDictionaryFactory>(),
                        factory.GetRequiredService<IUmbracoComponentRenderer>(), factory.GetRequiredService<IPublishedContentQuery>(), factory.GetRequiredService<MembershipHelper>());
                }

                return new UmbracoHelper();
            });

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
            // composition.WithCollectionBuilder<SurfaceControllerTypeCollectionBuilder>()
            //     .Add(composition.TypeLoader.GetSurfaceControllers());


            // auto-register views
            //composition.RegisterAuto(typeof(UmbracoViewPage<>));
        }
    }
}

