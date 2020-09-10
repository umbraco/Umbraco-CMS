using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.Sections;

namespace Umbraco.Web.Sections
{
    public class SectionCollectionBuilder : OrderedCollectionBuilderBase<SectionCollectionBuilder, SectionCollection, ISection>
    {
        protected override SectionCollectionBuilder This => this;

        protected override IEnumerable<ISection> CreateItems(IServiceProvider serviceProvider)
        {
            // get the manifest parser just-in-time - injecting it in the ctor would mean that
            // simply getting the builder in order to configure the collection, would require
            // its dependencies too, and that can create cycles or other oddities
            var manifestParser = serviceProvider.GetRequiredService<IManifestParser>();

            return base.CreateItems(serviceProvider).Concat(manifestParser.Manifest.Sections);
        }
    }
}
