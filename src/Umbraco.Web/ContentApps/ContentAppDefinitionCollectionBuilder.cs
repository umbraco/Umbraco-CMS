using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.ContentEditing;

namespace Umbraco.Web.ContentApps
{
    public class ContentAppDefinitionCollectionBuilder : OrderedCollectionBuilderBase<ContentAppDefinitionCollectionBuilder, ContentAppDefinitionCollection, IContentAppDefinition>
    {
        public ContentAppDefinitionCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override ContentAppDefinitionCollectionBuilder This => this;

        // need to inject dependencies in the collection, so override creation
        public override ContentAppDefinitionCollection CreateCollection()
        {
            // get the logger just-in-time - see note below for manifest parser
            var logger = Container.GetInstance<ILogger>();

            return new ContentAppDefinitionCollection(CreateItems(), logger);
        }

        protected override IEnumerable<IContentAppDefinition> CreateItems(params object[] args)
        {
            // get the manifest parser just-in-time - injecting it in the ctor would mean that
            // simply getting the builder in order to configure the collection, would require
            // its dependencies too, and that can create cycles or other oddities
            var manifestParser = Container.GetInstance<ManifestParser>();

            return base.CreateItems(args).Concat(manifestParser.Manifest.ContentApps);
        }
    }
}
