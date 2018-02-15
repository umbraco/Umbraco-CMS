using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.PropertyEditors
{
    public class PropertyEditorCollectionBuilder : LazyCollectionBuilderBase<PropertyEditorCollectionBuilder, PropertyEditorCollection, IConfiguredDataEditor>
    {
        private readonly ManifestParser _manifestParser;

        public PropertyEditorCollectionBuilder(IServiceContainer container, ManifestParser manifestParser)
            : base(container)
        {
            _manifestParser = manifestParser;
        }

        protected override PropertyEditorCollectionBuilder This => this;

        protected override IEnumerable<IConfiguredDataEditor> CreateItems(params object[] args)
        {
            return base.CreateItems(args)
                .Where(x => (x.Type & EditorType.PropertyValue) > 0)
                .Union(_manifestParser.Manifest.PropertyEditors);
        }
    }
}
