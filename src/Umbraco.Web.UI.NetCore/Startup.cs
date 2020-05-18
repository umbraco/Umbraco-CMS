using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Web.UI.BackOffice
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="webHostEnvironment"></param>
        /// <param name="config"></param>
        /// <remarks>
        /// Only a few services are possible to be injected here https://github.com/dotnet/aspnetcore/issues/9337
        /// </remarks>
        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _env = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // TODO: We will need to decide on if we want to use the ServiceBasedControllerActivator to create our controllers
            // or use the default IControllerActivator: DefaultControllerActivator (which doesn't directly use the container to resolve controllers)
            // This will affect whether we need to explicitly register controllers in the container like we do today in v8.
            // What we absolutely must do though is make sure we explicitly opt-in to using one or the other *always* for our controllers instead of
            // relying on a global configuration set by a user since if a custom IControllerActivator is used for our own controllers we may not
            // guarantee it will work. And then... is that even possible?

            services.AddUmbracoConfiguration(_config);
            services.AddUmbracoCore(_env, out var factory);
            services.AddUmbracoWebComponents();
            services.AddUmbracoRuntimeMinifier(_config);

            services.AddMvc();

            services.AddMiniProfiler(options =>
            {
                options.ShouldProfile = request => false; // WebProfiler determine and start profiling. We should not use the MiniProfilerMiddleware to also profile
            });

            // If using Kestrel: https://stackoverflow.com/a/55196057
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            //app.UseMiniProfiler();
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePages();
            app.UseRouting();

            app.UseUmbracoCore();
            app.UseUmbracoRouting();
            app.UseUmbracoRequestLogging();
            app.UseUmbracoWebsite();
            app.UseUmbracoBackOffice();
            app.UseUmbracoInstaller();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
