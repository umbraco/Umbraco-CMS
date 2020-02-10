using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Runtime;
using Umbraco.Web.Services;

namespace Umbraco.Web.Runtime
{
    // web's initial composer composes after core's, and before all core composers
    [ComposeAfter(typeof(CoreInitialComposer))]
    [ComposeBefore(typeof(ICoreComposer))]
    public sealed class BackOfficeComposer : ComponentComposer<BackOfficeComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.RegisterUnique<IDashboardService, DashboardService>();

            // register core CMS dashboards and 3rd party types - will be ordered by weight attribute & merged with package.manifest dashboards
            composition.Dashboards()
                .Add(composition.TypeLoader.GetTypes<IDashboard>());
        }
    }
}

