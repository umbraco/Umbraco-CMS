namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Provides a mapping function for <see cref="UmbracoDatabase.ExecuteScalar{T}(string, object[])" />
/// </summary>
public interface IScalarMapper
{
    /// <summary>
    ///     Maps the specified scalar value to a corresponding object representation.
    /// </summary>
    /// <param name="value">The scalar value to map.</param>
    /// <returns>The mapped object.</returns>
    object Map(object value);
}
