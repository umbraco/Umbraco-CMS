using Umbraco.Core.Composing;

namespace Umbraco.Web.Editors
{
    internal class EditorValidatorCollectionBuilder : LazyCollectionBuilderBase<EditorValidatorCollectionBuilder, EditorValidatorCollection, IEditorValidator>
    {
        public EditorValidatorCollectionBuilder(IContainer container)
            : base(container)
        { }

        protected override EditorValidatorCollectionBuilder This => this;
    }
}
