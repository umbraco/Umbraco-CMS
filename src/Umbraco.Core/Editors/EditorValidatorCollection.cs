using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Editors;

/// <summary>
/// Represents a collection of <see cref="IEditorValidator"/> instances.
/// </summary>
/// <remarks>
/// This collection is used to validate editor models in the backoffice.
/// </remarks>
public class EditorValidatorCollection : BuilderCollectionBase<IEditorValidator>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditorValidatorCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection of editor validators.</param>
    public EditorValidatorCollection(Func<IEnumerable<IEditorValidator>> items)
        : base(items)
    {
    }
}
