namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Provides a mapping function for <see cref="UmbracoDatabase.ExecuteScalar{T}(string, object[])" />
/// </summary>
public interface IScalarMapper
{
    /// <summary>
    ///     Performs a mapping operation for a scalar value.
    /// </summary>
    object Map(object value);
}
