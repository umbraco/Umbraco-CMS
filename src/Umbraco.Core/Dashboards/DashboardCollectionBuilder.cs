using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Cms.Core.Dashboards;

public class DashboardCollectionBuilder : WeightedCollectionBuilderBase<DashboardCollectionBuilder, DashboardCollection, IDashboard>
{
    protected override DashboardCollectionBuilder This => this;

    protected override IEnumerable<IDashboard> CreateItems(IServiceProvider factory)
    {
        // get the manifest parser just-in-time - injecting it in the ctor would mean that
        // simply getting the builder in order to configure the collection, would require
        // its dependencies too, and that can create cycles or other oddities
        ILegacyManifestParser legacyManifestParser = factory.GetRequiredService<ILegacyManifestParser>();

        IEnumerable<IDashboard> dashboardSections =
            Merge(base.CreateItems(factory), legacyManifestParser.CombinedManifest.Dashboards);

        return dashboardSections;
    }

    private IEnumerable<IDashboard> Merge(
        IEnumerable<IDashboard> dashboardsFromCode,
        IReadOnlyList<LegacyManifestDashboard> dashboardFromManifest) =>
        dashboardsFromCode.Concat(dashboardFromManifest)
            .Where(x => !string.IsNullOrEmpty(x.Alias))
            .OrderBy(GetWeight);

    private int GetWeight(IDashboard dashboard)
    {
        switch (dashboard)
        {
            case LegacyManifestDashboard manifestDashboardDefinition:
                return manifestDashboardDefinition.Weight;

            default:
                return GetWeight(dashboard.GetType());
        }
    }
}
