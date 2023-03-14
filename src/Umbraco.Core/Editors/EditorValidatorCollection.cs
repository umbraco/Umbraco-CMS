using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Editors;

public class EditorValidatorCollection : BuilderCollectionBase<IEditorValidator>
{
    public EditorValidatorCollection(Func<IEnumerable<IEditorValidator>> items)
        : base(items)
    {
    }
}
