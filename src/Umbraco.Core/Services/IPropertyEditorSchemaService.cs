using System.Text.Json.Nodes;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for querying property editor value schemas.
/// </summary>
/// <remarks>
/// <para>
/// This service enables retrieval of JSON Schema information for property editor values,
/// supporting programmatic content creation, import validation, and tooling.
/// </para>
/// <para>
/// Schema information is only available for property editors that implement <see cref="IValueSchemaProvider"/>.
/// </para>
/// </remarks>
public interface IPropertyEditorSchemaService : IService
{
    /// <summary>
    /// Gets the CLR type for a specific data type's stored values.
    /// </summary>
    /// <param name="dataTypeKey">The unique key of the data type.</param>
    /// <returns>
    /// The CLR type of values stored by this data type, or <c>null</c> if the type cannot be determined
    /// or the data type's property editor doesn't implement <see cref="IValueSchemaProvider"/>.
    /// </returns>
    Task<Type?> GetValueTypeAsync(Guid dataTypeKey);

    /// <summary>
    /// Gets the JSON Schema for a specific data type's stored values.
    /// </summary>
    /// <param name="dataTypeKey">The unique key of the data type.</param>
    /// <returns>
    /// A JSON Schema (draft 2020-12) describing the value structure, or <c>null</c> if the schema cannot be generated
    /// or the data type's property editor doesn't implement <see cref="IValueSchemaProvider"/>.
    /// </returns>
    Task<JsonObject?> GetValueSchemaAsync(Guid dataTypeKey);

    /// <summary>
    /// Gets the CLR type for a property editor with the specified configuration.
    /// </summary>
    /// <param name="propertyEditorAlias">The alias of the property editor.</param>
    /// <param name="configuration">The configuration object for the data type.</param>
    /// <returns>
    /// The CLR type, or <c>null</c> if the property editor doesn't implement <see cref="IValueSchemaProvider"/>.
    /// </returns>
    Type? GetValueType(string propertyEditorAlias, object? configuration);

    /// <summary>
    /// Gets the JSON Schema for a property editor with the specified configuration.
    /// </summary>
    /// <param name="propertyEditorAlias">The alias of the property editor.</param>
    /// <param name="configuration">The configuration object for the data type.</param>
    /// <returns>
    /// A JSON Schema (draft 2020-12), or <c>null</c> if the property editor doesn't implement <see cref="IValueSchemaProvider"/>.
    /// </returns>
    JsonObject? GetValueSchema(string propertyEditorAlias, object? configuration);

    /// <summary>
    /// Checks whether a property editor supports schema information.
    /// </summary>
    /// <param name="propertyEditorAlias">The alias of the property editor.</param>
    /// <returns><c>true</c> if the editor implements <see cref="IValueSchemaProvider"/>; otherwise, <c>false</c>.</returns>
    bool SupportsSchema(string propertyEditorAlias);
}
