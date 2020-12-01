using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Builder;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.Middleware;
using Umbraco.Web.BackOffice.Routing;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.BackOffice.Services;
using Umbraco.Web.BackOffice.Trees;
using Umbraco.Web.Common.Runtime;

namespace Umbraco.Web.BackOffice.Runtime
{
    [ComposeBefore(typeof(ICoreComposer))]
    [ComposeAfter(typeof(AspNetCoreComposer))]
    public class BackOfficeComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<BackOfficeAreaRoutes>();
            builder.Services.AddUnique<PreviewRoutes>();
            builder.Services.AddUnique<BackOfficeServerVariables>();
            builder.Services.AddScoped<BackOfficeSessionIdValidator>();
            builder.Services.AddScoped<BackOfficeSecurityStampValidator>();

            builder.Services.AddUnique<PreviewAuthenticationMiddleware>();
            builder.Services.AddUnique<BackOfficeExternalLoginProviderErrorMiddleware>();
            builder.Services.AddUnique<IBackOfficeAntiforgery, BackOfficeAntiforgery>();

            // register back office trees
            // the collection builder only accepts types inheriting from TreeControllerBase
            // and will filter out those that are not attributed with TreeAttribute
            var umbracoApiControllerTypes = builder.TypeLoader.GetUmbracoApiControllers().ToList();
            builder.Trees()
                .AddTreeControllers(umbracoApiControllerTypes.Where(x => typeof(TreeControllerBase).IsAssignableFrom(x)));

            builder.ComposeWebMappingProfiles();

            builder.Services.AddUnique<IPhysicalFileSystem>(factory =>
            {
                var path = "~/";
                var hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();
                return new PhysicalFileSystem(
                    factory.GetRequiredService<IIOHelper>(),
                    hostingEnvironment,
                    factory.GetRequiredService<ILogger<PhysicalFileSystem>>(),
                    hostingEnvironment.MapPathContentRoot(path),
                    hostingEnvironment.ToAbsolute(path)
                );
            });

            builder.Services.AddUnique<IIconService, IconService>();
            builder.Services.AddUnique<UnhandledExceptionLoggerMiddleware>();
        }
    }
}
