namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Represents a cache for <see cref="Umbraco.Cms.Core.Models.IDataType" /> configuration.
/// </summary>
public interface IDataTypeConfigurationCache
{
    /// <summary>
    /// Gets the data type configuration.
    /// </summary>
    /// <param name="id">The data type ID.</param>
    /// <returns>
    /// The data type configuration.
    /// </returns>
    object? GetConfiguration(int id) => GetConfigurationAs<object>(id);

    /// <summary>
    /// Gets the data type configuration as <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The data type configuration type.</typeparam>
    /// <param name="id">The data type ID.</param>
    /// <returns>
    /// The data type configuration as <typeparamref name="T" />.
    /// </returns>
    T? GetConfigurationAs<T>(int id)
        where T : class;
}
