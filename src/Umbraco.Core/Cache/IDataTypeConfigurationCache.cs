namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Represents a cache for <see cref="Umbraco.Cms.Core.Models.IDataType" /> configuration.
/// </summary>
public interface IDataTypeConfigurationCache
{
    /// <summary>
    /// Gets the data type configuration.
    /// </summary>
    /// <param name="key">The data type key.</param>
    /// <returns>
    /// The data type configuration.
    /// </returns>
    object? GetConfiguration(Guid key) => GetConfigurationAs<object>(key);

    /// <summary>
    /// Gets the data type configuration as <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The data type configuration type.</typeparam>
    /// <param name="key">The data type key.</param>
    /// <returns>
    /// The data type configuration as <typeparamref name="T" />.
    /// </returns>
    T? GetConfigurationAs<T>(Guid key)
        where T : class;

    /// <summary>
    /// Clears the cache for the specified keys.
    /// </summary>
    /// <param name="keys">The keys.</param>
    void ClearCache(IEnumerable<Guid> keys);
}
