namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
///     Defines a handler for discovering sub-types for polymorphic OpenAPI schemas.
/// </summary>
public interface ISubTypesHandler
{
    /// <summary>
    ///     Determines whether this handler can discover sub-types for the specified type and document.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="documentName">The OpenAPI document name.</param>
    /// <returns><c>true</c> if this handler can handle the type; otherwise, <c>false</c>.</returns>
    bool CanHandle(Type type, string documentName);

    /// <summary>
    ///     Discovers sub-types for the specified type.
    /// </summary>
    /// <param name="type">The type to discover sub-types for.</param>
    /// <returns>An enumerable of discovered sub-types.</returns>
    IEnumerable<Type> Handle(Type type);
}
