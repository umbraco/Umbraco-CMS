namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Handles schema ID generation for specific types.
/// </summary>
public interface ISchemaIdHandler
{
    /// <summary>
    /// Determines whether this handler can generate a schema ID for the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if this handler can generate a schema ID for the type; otherwise, <c>false</c>.</returns>
    bool CanHandle(Type type);

    /// <summary>
    /// Generates a schema ID for the specified type.
    /// </summary>
    /// <param name="type">The type for which to generate the schema ID.</param>
    /// <returns>The schema ID for the specified type.</returns>
    string Handle(Type type);
}
