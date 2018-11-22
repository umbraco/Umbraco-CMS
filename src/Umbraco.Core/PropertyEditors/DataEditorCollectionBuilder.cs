using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class DataEditorCollectionBuilder : LazyCollectionBuilderBase<DataEditorCollectionBuilder, DataEditorCollection, IDataEditor>
    {
        public DataEditorCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override DataEditorCollectionBuilder This => this;
    }
}
