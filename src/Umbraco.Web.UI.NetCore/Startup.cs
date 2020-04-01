using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.BackOffice.AspNetCore;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Extensions;
using Umbraco.Web.Website.AspNetCore;
using IHostingEnvironment = Umbraco.Core.Hosting.IHostingEnvironment;


namespace Umbraco.Web.UI.BackOffice
{
    public class Startup
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
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
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUmbracoConfiguration(_config);
            services.AddUmbracoRuntimeMinifier(_config);
            services.AddUmbracoCore(_webHostEnvironment, out var factory);
            services.AddUmbracoWebsite();

            services.AddMvc();
            services.AddMiniProfiler(options =>
            {
                options.ShouldProfile = request => false; // WebProfiler determine and start profiling. We should not use the MiniProfilerMiddleware to also profile
            });

            //Finally initialize Current
            Current.Initialize(
                factory.GetInstance<ILogger> (),
                factory.GetInstance<Configs>(),
                factory.GetInstance<IIOHelper>(),
                factory.GetInstance<IHostingEnvironment>(),
                factory.GetInstance<IBackOfficeInfo>(),
                factory.GetInstance<IProfiler>()
            );

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        //    app.UseMiniProfiler();
            app.UseUmbracoRequest();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseUmbracoCore();
            app.UseUmbracoWebsite();
            app.UseUmbracoBackOffice();
            app.UseRouting();
            app.UseUmbracoRuntimeMinification();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("Backoffice", "/umbraco/{Action}", new
                {
                    Controller = "BackOffice",
                    Action = "Default"
                });
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync($"<html><body>Hello World!{Current.Profiler.Render()}</body></html>");
                });
            });
        }
    }
}
