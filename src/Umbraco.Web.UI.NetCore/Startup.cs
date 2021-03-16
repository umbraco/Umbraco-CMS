using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Web.BackOffice.ModelsBuilder;
using Microsoft.Identity.Web;
using Umbraco.Extensions;
using Umbraco.Cms.Web.BackOffice.Security;
using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Web.UI.NetCore
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="webHostEnvironment">The Web Host Environment</param>
        /// <param name="config">The Configuration</param>
        /// <remarks>
        /// Only a few services are possible to be injected here https://github.com/dotnet/aspnetcore/issues/9337
        /// </remarks>
        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _env = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }



        /// <summary>
        /// Configures the services
        /// </summary>
        /// <remarks>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </remarks>
        public void ConfigureServices(IServiceCollection services)
        {
#pragma warning disable IDE0022 // Use expression body for methods
            services.AddUmbraco(_env, _config)
                .AddBackOffice()
                .AddBackOfficeExternalLogins(logins =>
                {
                    var loginProviderOptions = new BackOfficeExternalLoginProviderOptions(
                        "btn-google",
                        "fa-google",
                        new ExternalSignInAutoLinkOptions(true)
                        {
                            OnAutoLinking = (user, login) =>
                            {
                            },
                            OnExternalLogin = (user, login) =>
                            {
                                user.Claims.Add(new IdentityUserClaim<string>
                                {
                                    ClaimType = "hello",
                                    ClaimValue = "world"
                                });
                                return true;
                            }
                        },
                        denyLocalLogin: false,
                        autoRedirectLoginToExternalProvider: false);

                    logins.AddBackOfficeLogin(
                        loginProviderOptions,
                        auth =>
                        {
                            auth.AddGoogle(
                                auth.SchemeForBackOffice("Google"), // The scheme must be set with this method to work for the back office
                                options =>
                                {
                                    // By default this is '/signin-google' but it needs to be changed to this
                                    options.CallbackPath = "/umbraco-google-signin";
                                    options.ClientId = "1072120697051-p41pro11srud3o3n90j7m00geq426jqt.apps.googleusercontent.com";
                                    options.ClientSecret = "cs_LJTXh2rtI01C5OIt9WFkt";
                                });

                            // NOTE: Adding additional providers here is possible via the API but
                            // it will mean that the same BackOfficeExternalLoginProviderOptions will be registered
                            // for them. In some weird cases maybe people would want that?
                        });

                    logins.AddBackOfficeLogin(
                        new BackOfficeExternalLoginProviderOptions("btn-microsoft", "fa-windows"),
                        auth =>
                        {
                            auth.AddMicrosoftIdentityWebApp(
                                options =>
                                {
                                    options.SaveTokens = true;

                                    // By default this is '/signin-oidc' but it needs to be changed to this
                                    options.CallbackPath = "/umbraco-signin-oidc";
                                    options.Instance = "https://login.microsoftonline.com/";
                                    options.TenantId = "3bb0b4c5-364f-4394-ad36-0f29f95e5ddd";
                                    options.ClientId = "56e98cad-ed2d-4f1b-8f85-bef11adc163f";
                                    options.ClientSecret = "-1E9_fdPHi_ZkSQOb2.O5LG025sv6-NQ3h";
                                },
                                openIdConnectScheme: auth.SchemeForBackOffice("AzureAD"), // The scheme must be set with this method to work for the back office
                                cookieScheme: "Fake");
                        });
                })
                .AddWebsite()
                .AddComposers()
                .Build();
#pragma warning restore IDE0022 // Use expression body for methods

        }

        /// <summary>
        /// Configures the application
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            app.UseUmbracoBackOffice();
            app.UseUmbracoWebsite();
        }
    }
}
