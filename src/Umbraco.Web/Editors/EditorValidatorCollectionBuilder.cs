using Umbraco.Core.Composing;

namespace Umbraco.Web.Editors
{
    internal class EditorValidatorCollectionBuilder : LazyCollectionBuilderBase<EditorValidatorCollectionBuilder, EditorValidatorCollection, IEditorValidator>
    {
        protected override EditorValidatorCollectionBuilder This => this;
    }
}
