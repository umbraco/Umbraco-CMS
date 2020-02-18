using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class DataEditorWithMediaPathCollectionBuilder : LazyCollectionBuilderBase<DataEditorWithMediaPathCollectionBuilder, DataEditorWithMediaPathCollection,  IDataEditorWithMediaPath>
    {
        protected override DataEditorWithMediaPathCollectionBuilder This => this;
    }
}
