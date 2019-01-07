using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class DataEditorCollectionBuilder : LazyCollectionBuilderBase<DataEditorCollectionBuilder, DataEditorCollection, IDataEditor>
    {
        protected override DataEditorCollectionBuilder This => this;
    }
}
