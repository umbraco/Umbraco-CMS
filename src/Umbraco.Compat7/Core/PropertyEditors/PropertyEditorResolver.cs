using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using CoreCurrent = Umbraco.Core.Composing.Current;
using LightInject;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.PropertyEditors
{
    public class PropertyEditorResolver : LazyManyObjectsResolverBase<PropertyEditorCollectionBuilder, PropertyEditorCollection, ConfiguredDataEditor>
    {
        private PropertyEditorResolver(PropertyEditorCollectionBuilder builder)
            : base(builder)
        { }

        public static PropertyEditorResolver Current { get; }
            = new PropertyEditorResolver(CoreCurrent.Container.GetInstance<PropertyEditorCollectionBuilder>());

        public IEnumerable<ConfiguredDataEditor> PropertyEditors => CoreCurrent.PropertyEditors;

        public ConfiguredDataEditor GetByAlias(string alias) => CoreCurrent.PropertyEditors[alias];
    }
}
