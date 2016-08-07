using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.PropertyEditors
{
    internal class PropertyEditorCollectionBuilder : LazyCollectionBuilderBase<PropertyEditorCollectionBuilder, PropertyEditorCollection, PropertyEditor>
    {
        private readonly ManifestBuilder _manifestBuilder;

        public PropertyEditorCollectionBuilder(IServiceContainer container, ManifestBuilder manifestBuilder)
            : base(container)
        {
            _manifestBuilder = manifestBuilder;
        }

        protected override PropertyEditorCollectionBuilder This => this;

        protected override IEnumerable<PropertyEditor> CreateItems(params object[] args)
        {
            return base.CreateItems(args).Union(_manifestBuilder.PropertyEditors);
        }
    }
}
