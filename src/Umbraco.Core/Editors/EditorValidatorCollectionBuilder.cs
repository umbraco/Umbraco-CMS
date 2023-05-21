using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Editors;

public class EditorValidatorCollectionBuilder : LazyCollectionBuilderBase<EditorValidatorCollectionBuilder,
    EditorValidatorCollection, IEditorValidator>
{
    protected override EditorValidatorCollectionBuilder This => this;
}
