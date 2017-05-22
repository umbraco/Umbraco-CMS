using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using CoreCurrent = Umbraco.Core.DI.Current;
using LightInject;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.PropertyEditors
{
    public class PropertyEditorResolver : LazyManyObjectsResolverBase<PropertyEditorCollectionBuilder, PropertyEditorCollection, PropertyEditor>
    {
        private PropertyEditorResolver(PropertyEditorCollectionBuilder builder)
            : base(builder)
        { }

        public static PropertyEditorResolver Current { get; }
            = new PropertyEditorResolver(CoreCurrent.Container.GetInstance<PropertyEditorCollectionBuilder>());

        public IEnumerable<PropertyEditor> PropertyEditors => CoreCurrent.PropertyEditors;

        public PropertyEditor GetByAlias(string alias) => CoreCurrent.PropertyEditors[alias];
    }
}
