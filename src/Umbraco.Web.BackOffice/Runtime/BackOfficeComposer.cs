using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Routing;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.BackOffice.Services;
using Umbraco.Web.BackOffice.Trees;
using Umbraco.Web.Common.Runtime;
using Umbraco.Web.Trees;

namespace Umbraco.Web.BackOffice.Runtime
{
    [ComposeBefore(typeof(ICoreComposer))]
    [ComposeAfter(typeof(AspNetCoreComposer))]
    public class BackOfficeComposer : IComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<BackOfficeAreaRoutes>();
            composition.RegisterUnique<PreviewRoutes>();
            composition.RegisterUnique<BackOfficeServerVariables>();
            composition.Services.AddScoped<BackOfficeSessionIdValidator>();
            composition.Services.AddScoped<BackOfficeSecurityStampValidator>();

            composition.RegisterUnique<PreviewAuthenticationMiddleware>();
            composition.RegisterUnique<IBackOfficeAntiforgery, BackOfficeAntiforgery>();

            // register back office trees
            // the collection builder only accepts types inheriting from TreeControllerBase
            // and will filter out those that are not attributed with TreeAttribute
            var umbracoApiControllerTypes = composition.TypeLoader.GetUmbracoApiControllers().ToList();
            composition.Trees()
                .AddTreeControllers(umbracoApiControllerTypes.Where(x => typeof(TreeControllerBase).IsAssignableFrom(x)));

            composition.ComposeWebMappingProfiles();

            composition.Services.AddUnique<IPhysicalFileSystem, PhysicalFileSystem>(factory =>
                new PhysicalFileSystem(
                    factory.GetRequiredService<IIOHelper>(),
                    factory.GetRequiredService<IHostingEnvironment>(),
                    factory.GetRequiredService<ILogger<PhysicalFileSystem>>(),
                    "~/"));

            composition.RegisterUnique<IIconService, IconService>();
        }
    }
}
