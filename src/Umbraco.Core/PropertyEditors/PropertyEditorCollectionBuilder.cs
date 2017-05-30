using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.PropertyEditors
{
    public class PropertyEditorCollectionBuilder : LazyCollectionBuilderBase<PropertyEditorCollectionBuilder, PropertyEditorCollection, PropertyEditor>
    {
        public PropertyEditorCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        // have to property-inject that one as it is internal & the builder is public
        [Inject]
        internal ManifestBuilder ManifestBuilder { get; set; }

        protected override PropertyEditorCollectionBuilder This => this;

        protected override IEnumerable<PropertyEditor> CreateItems(params object[] args)
        {
            return base.CreateItems(args).Union(ManifestBuilder.PropertyEditors);
        }
    }
}
