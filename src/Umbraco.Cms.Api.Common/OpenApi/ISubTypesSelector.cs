namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
///     Defines a selector for choosing sub-types from registered handlers.
/// </summary>
public interface ISubTypesSelector
{
    /// <summary>
    ///     Selects sub-types for the specified type for polymorphic OpenAPI schema generation.
    /// </summary>
    /// <param name="type">The type to find sub-types for.</param>
    /// <returns>An enumerable of sub-types.</returns>
    IEnumerable<Type> SubTypes(Type type);
}
