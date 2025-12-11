namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Selects the appropriate schema ID for a type by delegating to registered handlers.
/// </summary>
public interface ISchemaIdSelector
{
    /// <summary>
    /// Gets the schema ID for the specified type.
    /// </summary>
    /// <param name="type">The type for which to get the schema ID.</param>
    /// <returns>The schema ID for the specified type.</returns>
    string SchemaId(Type type);
}
