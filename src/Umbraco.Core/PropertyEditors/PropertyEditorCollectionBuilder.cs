using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.PropertyEditors
{
    public class PropertyEditorCollectionBuilder : LazyCollectionBuilderBase<PropertyEditorCollectionBuilder, PropertyEditorCollection, PropertyEditor>
    {
        private readonly ManifestParser _manifestParser;

        public PropertyEditorCollectionBuilder(IServiceContainer container, ManifestParser manifestParser)
            : base(container)
        {
            _manifestParser = manifestParser;
        }

        protected override PropertyEditorCollectionBuilder This => this;

        protected override IEnumerable<PropertyEditor> CreateItems(params object[] args)
        {
            return base.CreateItems(args).Union(_manifestParser.Manifest.PropertyEditors);
        }
    }
}
