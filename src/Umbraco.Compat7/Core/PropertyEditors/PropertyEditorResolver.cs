using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using CoreCurrent = Umbraco.Core.Composing.Current;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.PropertyEditors
{
    public class PropertyEditorResolver : LazyManyObjectsResolverBase<DataEditorCollectionBuilder, DataEditorCollection, IDataEditor>
    {
        private PropertyEditorResolver(DataEditorCollectionBuilder builder)
            : base(builder)
        { }

        public static PropertyEditorResolver Current { get; }
            = new PropertyEditorResolver(CoreCurrent.Container.GetInstance<DataEditorCollectionBuilder>());

        public IEnumerable<IDataEditor> PropertyEditors => CoreCurrent.PropertyEditors;

        public IDataEditor GetByAlias(string alias) => CoreCurrent.PropertyEditors[alias];
    }
}
