using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.ContentEditing;

namespace Umbraco.Web.ContentApps
{
    public class ContentAppDefinitionCollectionBuilder : OrderedCollectionBuilderBase<ContentAppDefinitionCollectionBuilder, ContentAppDefinitionCollection, IContentAppDefinition>
    {
        private readonly ManifestParser _manifestParser;

        public ContentAppDefinitionCollectionBuilder(IServiceContainer container, ManifestParser manifestParser)
            : base(container)
        {
            _manifestParser = manifestParser;
        }

        protected override ContentAppDefinitionCollectionBuilder This => this;

        protected override IEnumerable<IContentAppDefinition> CreateItems(params object[] args)
        {
            return base.CreateItems(args).Concat(_manifestParser.Manifest.ContentApps);
        }
    }
}
