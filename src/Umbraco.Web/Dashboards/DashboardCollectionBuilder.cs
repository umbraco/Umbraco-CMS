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

            //TODO WB: We will need to re-sort items from package manifest with the C# Types
            return base.CreateItems(factory).Concat(manifestParser.Manifest.Dashboards);
        }
    }
}
