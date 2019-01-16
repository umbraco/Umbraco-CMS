using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.ContentEditing;

namespace Umbraco.Web.ContentApps
{
    public class ContentAppFactoryCollectionBuilder : OrderedCollectionBuilderBase<ContentAppFactoryCollectionBuilder, ContentAppFactoryCollection, IContentAppFactory>
    {
        protected override ContentAppFactoryCollectionBuilder This => this;

        // need to inject dependencies in the collection, so override creation
        public override ContentAppFactoryCollection CreateCollection(IFactory factory)
        {
            // get the logger just-in-time - see note below for manifest parser
            var logger = factory.GetInstance<ILogger>();

            return new ContentAppFactoryCollection(CreateItems(factory), logger);
        }

        protected override IEnumerable<IContentAppFactory> CreateItems(IFactory factory)
        {
            // get the manifest parser just-in-time - injecting it in the ctor would mean that
            // simply getting the builder in order to configure the collection, would require
            // its dependencies too, and that can create cycles or other oddities
            var manifestParser = factory.GetInstance<ManifestParser>();

            return base.CreateItems(factory).Concat(manifestParser.Manifest.ContentApps.Select(x => new ManifestContentAppFactory(x)));
        }
    }
}
