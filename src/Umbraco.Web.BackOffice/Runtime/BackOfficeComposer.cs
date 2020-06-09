using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Routing;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.BackOffice.Trees;
using Umbraco.Web.Common.Runtime;

namespace Umbraco.Web.BackOffice.Runtime
{
    [ComposeBefore(typeof(ICoreComposer))]
    [ComposeAfter(typeof(AspNetCoreComposer))]
    public class BackOfficeComposer : IComposer
    {
        public void Compose(Composition composition)
        {


            composition.RegisterUnique<BackOfficeAreaRoutes>();
            composition.RegisterUnique<BackOfficeServerVariables>();
            composition.Register<BackOfficeSessionIdValidator>(Lifetime.Request);
            composition.Register<BackOfficeSecurityStampValidator>(Lifetime.Request);

            composition.RegisterUnique<IBackOfficeAntiforgery, BackOfficeAntiforgery>();

            // register back office trees
            // the collection builder only accepts types inheriting from TreeControllerBase
            // and will filter out those that are not attributed with TreeAttribute
            var umbracoApiControllerTypes = composition.TypeLoader.GetUmbracoApiControllers().ToList();
            composition.Trees()
                .AddTreeControllers(umbracoApiControllerTypes.Where(x => typeof(TreeControllerBase).IsAssignableFrom(x)));


            composition.ComposeWebMappingProfiles();
        }
    }
}
