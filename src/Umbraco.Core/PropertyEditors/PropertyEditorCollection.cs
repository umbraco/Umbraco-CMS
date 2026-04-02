using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a collection of property editors (<see cref="IDataEditor"/>).
/// </summary>
public class PropertyEditorCollection : BuilderCollectionBase<IDataEditor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyEditorCollection"/> class.
    /// </summary>
    /// <param name="dataEditors">The data editor collection.</param>
    public PropertyEditorCollection(DataEditorCollection dataEditors)
        : base(() => dataEditors)
    {
    }

    /// <summary>
    /// Gets the property editor with the specified alias.
    /// </summary>
    /// <param name="alias">The editor alias.</param>
    /// <returns>The property editor if found; otherwise, null.</returns>
    /// <remarks>Virtual so it can be mocked.</remarks>
    public virtual IDataEditor? this[string? alias]
        => this.SingleOrDefault(x => x.Alias == alias);

    /// <summary>
    /// Tries to get the property editor with the specified alias.
    /// </summary>
    /// <param name="alias">The editor alias.</param>
    /// <param name="editor">When this method returns, contains the editor if found; otherwise, null.</param>
    /// <returns><c>true</c> if the editor was found; otherwise, <c>false</c>.</returns>
    public virtual bool TryGet(string? alias, [MaybeNullWhen(false)] out IDataEditor editor)
    {
        editor = this.FirstOrDefault(x => x.Alias == alias);
        return editor != null;
    }
}
