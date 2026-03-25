using System.Text.Json.Nodes;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides schema information about the values a property editor accepts.
/// </summary>
/// <remarks>
/// <para>
/// This interface is opt-in for property editors that want to expose their value schema
/// for programmatic content creation, validation, and tooling support.
/// </para>
/// <para>
/// Implementations should return the schema for the incoming value (what <see cref="IDataValueEditor.FromEditor"/>
/// receives via <see cref="ContentPropertyData.Value"/>), not the stored/database model (what
/// <see cref="IDataValueEditor.FromEditor"/> produces) or the published model (what
/// <see cref="IPropertyValueConverter"/> produces).
/// </para>
/// </remarks>
public interface IValueSchemaProvider
{
    /// <summary>
    /// Gets the CLR type that represents the incoming value structure.
    /// </summary>
    /// <param name="configuration">The data type configuration, which may affect the value type.</param>
    /// <returns>
    /// The CLR type of the incoming value, or <c>null</c> if the type cannot be determined
    /// or varies significantly based on configuration.
    /// </returns>
    /// <remarks>
    /// <para>
    /// For simple editors (e.g., textbox), this might return <see cref="string"/>.
    /// For complex editors (e.g., MediaPicker3), this returns the DTO type that the editor submits.
    /// For block-based editors where the structure is entirely configuration-dependent, this may return <c>null</c>.
    /// </para>
    /// </remarks>
    Type? GetValueType(object? configuration);

    /// <summary>
    /// Gets a JSON Schema (draft 2020-12) describing the incoming value structure.
    /// </summary>
    /// <param name="configuration">The data type configuration, which may affect the schema.</param>
    /// <returns>
    /// A <see cref="JsonObject"/> containing a valid JSON Schema, or <c>null</c> if the schema
    /// cannot be generated for the given configuration.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The returned schema should describe the structure that <see cref="IDataValueEditor.FromEditor"/> receives
    /// (i.e., what the Management API accepts).
    /// </para>
    /// <para>
    /// For configuration-dependent schemas (e.g., BlockList with specific element types), the schema
    /// should reflect the constraints defined in the configuration.
    /// </para>
    /// </remarks>
    JsonObject? GetValueSchema(object? configuration);
}
