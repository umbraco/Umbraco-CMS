namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Specifies the type of composition relationship between content types.
/// </summary>
public enum CompositionType
{
    /// <summary>
    ///     Indicates a composition relationship where properties are included from another content type.
    /// </summary>
    Composition,

    /// <summary>
    ///     Indicates an inheritance relationship where the content type inherits from a parent content type.
    /// </summary>
    Inheritance
}
