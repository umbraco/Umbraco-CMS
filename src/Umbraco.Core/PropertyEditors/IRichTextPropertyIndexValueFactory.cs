namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property index value factory specifically for rich text properties.
/// </summary>
/// <remarks>
///     This marker interface allows for specialized indexing of rich text content,
///     which may include markup, embedded media, and block content.
/// </remarks>
public interface IRichTextPropertyIndexValueFactory : IPropertyIndexValueFactory
{
}
