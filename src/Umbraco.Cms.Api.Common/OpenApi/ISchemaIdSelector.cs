namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
///     Defines a selector for choosing schema IDs from registered handlers.
/// </summary>
public interface ISchemaIdSelector
{
    /// <summary>
    ///     Selects a schema ID for the specified type.
    /// </summary>
    /// <param name="type">The type to generate a schema ID for.</param>
    /// <returns>The schema ID.</returns>
    string SchemaId(Type type);
}
