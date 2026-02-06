using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides caching for <see cref="IDataValueEditor" /> instances associated with data types.
/// </summary>
/// <remarks>
///     This cache reduces the computational overhead of repeatedly creating value editors for
///     the same data type and editor combinations during request processing.
/// </remarks>
public interface IValueEditorCache
{
    /// <summary>
    ///     Gets the value editor for the specified data editor and data type combination.
    /// </summary>
    /// <param name="dataEditor">The data editor.</param>
    /// <param name="dataType">The data type.</param>
    /// <returns>The cached or newly created <see cref="IDataValueEditor" />.</returns>
    public IDataValueEditor GetValueEditor(IDataEditor dataEditor, IDataType dataType);

    /// <summary>
    ///     Clears the cached value editors for the specified data type identifiers.
    /// </summary>
    /// <param name="dataTypeIds">The data type identifiers to clear from the cache.</param>
    public void ClearCache(IEnumerable<int> dataTypeIds);
}
