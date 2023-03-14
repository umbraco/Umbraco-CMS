using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PropertyEditors;

public class
    DataEditorCollectionBuilder : LazyCollectionBuilderBase<DataEditorCollectionBuilder, DataEditorCollection, IDataEditor>
{
    protected override DataEditorCollectionBuilder This => this;
}
