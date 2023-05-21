using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Cms.Core.Sections;

public class
    SectionCollectionBuilder : OrderedCollectionBuilderBase<SectionCollectionBuilder, SectionCollection, ISection>
{
    protected override SectionCollectionBuilder This => this;

    protected override IEnumerable<ISection> CreateItems(IServiceProvider factory)
    {
        // get the manifest parser just-in-time - injecting it in the ctor would mean that
        // simply getting the builder in order to configure the collection, would require
        // its dependencies too, and that can create cycles or other oddities
        IManifestParser manifestParser = factory.GetRequiredService<IManifestParser>();

        return base.CreateItems(factory).Concat(manifestParser.CombinedManifest.Sections);
    }
}
