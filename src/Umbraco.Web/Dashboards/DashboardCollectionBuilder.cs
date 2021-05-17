using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Manifest;

namespace Umbraco.Web.Dashboards
{
    public class DashboardCollectionBuilder : WeightedCollectionBuilderBase<DashboardCollectionBuilder, DashboardCollection, IDashboard>
    {
        protected override DashboardCollectionBuilder This => this;

        protected override IEnumerable<IDashboard> CreateItems(IFactory factory)
        {
            // get the manifest parser just-in-time - injecting it in the ctor would mean that
            // simply getting the builder in order to configure the collection, would require
            // its dependencies too, and that can create cycles or other oddities
            var manifestParser = factory.GetInstance<ManifestParser>();

            var dashboardSections = Merge(base.CreateItems(factory), manifestParser.Manifest.Dashboards);

            return dashboardSections;
        }

        private IEnumerable<IDashboard> Merge(IEnumerable<IDashboard> dashboardsFromCode, IReadOnlyList<ManifestDashboard> dashboardFromManifest)
        {
            return dashboardsFromCode.Concat(dashboardFromManifest)
                .Where(x => !string.IsNullOrEmpty(x.Alias))
                .OrderBy(GetWeight);
        }

        private int GetWeight(IDashboard dashboard)
        {
            switch (dashboard)
            {
                case ManifestDashboard manifestDashboardDefinition:
                    return manifestDashboardDefinition.Weight;

                default:
                    return GetWeight(dashboard.GetType());
            }
        }
    }
}
