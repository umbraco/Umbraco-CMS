using LightInject;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Web.Editors
{
    internal class EditorValidatorCollectionBuilder : LazyCollectionBuilderBase<EditorValidatorCollectionBuilder, EditorValidatorCollection, IEditorValidator>
    {
        public EditorValidatorCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override EditorValidatorCollectionBuilder This => this;
    }
}
