namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
///     Defines a handler for generating OpenAPI schema IDs.
/// </summary>
public interface ISchemaIdHandler
{
    /// <summary>
    ///     Determines whether this handler can generate a schema ID for the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if this handler can handle the type; otherwise, <c>false</c>.</returns>
    bool CanHandle(Type type);

    /// <summary>
    ///     Generates a schema ID for the specified type.
    /// </summary>
    /// <param name="type">The type to generate a schema ID for.</param>
    /// <returns>The generated schema ID.</returns>
    string Handle(Type type);
}
