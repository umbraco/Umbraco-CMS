using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Manifest;

namespace Umbraco.Web.Dashboards
{
    public class DashboardCollectionBuilder : WeightedCollectionBuilderBase<DashboardCollectionBuilder, DashboardCollection, IDashboardSection>
    {
        protected override DashboardCollectionBuilder This => this;

        protected override IEnumerable<IDashboardSection> CreateItems(IFactory factory)
        {
            // get the manifest parser just-in-time - injecting it in the ctor would mean that
            // simply getting the builder in order to configure the collection, would require
            // its dependencies too, and that can create cycles or other oddities
            var manifestParser = factory.GetInstance<ManifestParser>();

            var dashboardSections = Merge(base.CreateItems(factory).ToArray(), manifestParser.Manifest.Dashboards);

            return dashboardSections;
        }

        private IEnumerable<IDashboardSection> Merge(IReadOnlyList<IDashboardSection> dashboardsFromCode, IReadOnlyList<ManifestDashboardDefinition> dashboardFromManifest)
        {
            var list = dashboardsFromCode.Concat(dashboardFromManifest)
                .Where(x=>!string.IsNullOrEmpty(x.Alias))
                .Select(x => (Weight: GetWeight(x), DashboardSection: x))
                .OrderBy(x=>x.Weight);

            return list.Select(x => x.DashboardSection);
        }

        private static int GetWeight(IDashboardSection dashboardSection)
        {
            switch (dashboardSection)
            {
                case ManifestDashboardDefinition danifestDashboardDefinition:
                    return danifestDashboardDefinition.Weight;
                default:
                    var weightAttribute = dashboardSection.GetType().GetCustomAttributes(typeof(WeightAttribute), false)
                        .Cast<WeightAttribute>().FirstOrDefault();
                    return weightAttribute.Weight;
            }
        }
    }
}
