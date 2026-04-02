namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents the subset of content type property information that is needed for schema generation.
/// </summary>
public class ContentTypePropertySchemaInfo
{
    /// <summary>
    /// Gets the property alias.
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// Gets the property editor alias.
    /// </summary>
    public required string EditorAlias { get; init; }

    /// <summary>
    /// Gets the CLR type used to represent this property in the Delivery API.
    /// </summary>
    public required Type DeliveryApiClrType { get; init; }

    /// <summary>
    /// Gets a value indicating whether this property is inherited from a composition.
    /// </summary>
    public bool Inherited { get; init; }
}
