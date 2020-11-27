using Umbraco.Core.Composing;

namespace Umbraco.Web.Editors
{
    public class EditorValidatorCollectionBuilder : LazyCollectionBuilderBase<EditorValidatorCollectionBuilder, EditorValidatorCollection, IEditorValidator>
    {
        protected override EditorValidatorCollectionBuilder This => this;
    }
}
