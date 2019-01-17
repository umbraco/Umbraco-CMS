using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    //fixme: how can a developer re-sort the items in this collection ?
    public class BackOfficeSectionCollectionBuilder : OrderedCollectionBuilderBase<BackOfficeSectionCollectionBuilder, BackOfficeSectionCollection, IBackOfficeSection>
    {
        protected override BackOfficeSectionCollectionBuilder This => this;

        protected override IEnumerable<IBackOfficeSection> CreateItems(IFactory factory)
        {
            // get the manifest parser just-in-time - injecting it in the ctor would mean that
            // simply getting the builder in order to configure the collection, would require
            // its dependencies too, and that can create cycles or other oddities
            var manifestParser = factory.GetInstance<ManifestParser>();

            return base.CreateItems(factory)
                .Concat(manifestParser.Manifest.Sections.Select(x => new ManifestBackOfficeSection(x.Key, x.Value)));
        }

        private class ManifestBackOfficeSection : IBackOfficeSection
        {
            public ManifestBackOfficeSection(string @alias, string name)
            {
                Alias = alias;
                Name = name;
            }

            public string Alias { get; }
            public string Name { get; }
        }
    }
}
