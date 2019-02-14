using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.Trees;

namespace Umbraco.Web.Trees
{
    public class BackOfficeSectionCollectionBuilder : OrderedCollectionBuilderBase<BackOfficeSectionCollectionBuilder, BackOfficeSectionCollection, IBackOfficeSection>
    {
        protected override BackOfficeSectionCollectionBuilder This => this;

        protected override IEnumerable<IBackOfficeSection> CreateItems(IFactory factory)
        {
            // get the manifest parser just-in-time - injecting it in the ctor would mean that
            // simply getting the builder in order to configure the collection, would require
            // its dependencies too, and that can create cycles or other oddities
            var manifestParser = factory.GetInstance<ManifestParser>();

            return base.CreateItems(factory).Concat(manifestParser.Manifest.Sections);
        }
    }
}
