using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a collection of <see cref="IDataEditor"/> instances.
/// </summary>
public class DataEditorCollection : BuilderCollectionBase<IDataEditor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataEditorCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection items.</param>
    public DataEditorCollection(Func<IEnumerable<IDataEditor>> items)
        : base(items)
    {
    }
}
