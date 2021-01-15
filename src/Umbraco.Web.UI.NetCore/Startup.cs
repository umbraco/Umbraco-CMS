using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Core.DependencyInjection;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.DependencyInjection;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.DependencyInjection;
using Umbraco.Web.Website.DependencyInjection;

namespace Umbraco.Web.UI.NetCore
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
                .AddWebsite()
                .AddComposers()
                // TODO: This call and AddDistributedCache are interesting ones. They are both required for back office and front-end to render
                // but we don't want to force people to call so many of these ext by default and want to keep all of this relatively simple.
                // but we still need to allow the flexibility for people to use their own ModelsBuilder. In that case people can call a different
                // AddModelsBuilderCommunity (or whatever) after our normal calls to replace our services.
                // So either we call AddModelsBuilder within AddBackOffice AND AddWebsite just like we do with AddDistributedCache or we
                // have a top level method to add common things required for backoffice/frontend like .AddCommon()
                // or we allow passing in options to these methods to configure what happens within them.
                .AddModelsBuilder()
                .Build();
#pragma warning restore IDE0022 // Use expression body for methods

        }

        /// <summary>
        /// Configures the application
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseUmbraco();
            app.UseUmbracoBackOffice();
            app.UseUmbracoWebsite();
        }
    }
}
