namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a simplified view of a content type.
/// </summary>
public interface ISimpleContentType
{
    int Id { get; }

    Guid Key { get; }

    string? Name { get; }

    /// <summary>
    ///     Gets the alias of the content type.
    /// </summary>
    string Alias { get; }

    /// <summary>
    ///     Gets the default template of the content type.
    /// </summary>
    ITemplate? DefaultTemplate { get; }

    /// <summary>
    ///     Gets the content variation of the content type.
    /// </summary>
    ContentVariation Variations { get; }

    /// <summary>
    ///     Gets the icon of the content type.
    /// </summary>
    string? Icon { get; }

    /// <summary>
    ///     Gets a value indicating whether the content type is a container.
    /// </summary>
    bool IsContainer { get; }

    /// <summary>
    ///     Gets a value indicating whether content of that type can be created at the root of the tree.
    /// </summary>
    bool AllowedAsRoot { get; }

    /// <summary>
    ///     Gets a value indicating whether the content type is an element content type.
    /// </summary>
    bool IsElement { get; }

    /// <summary>
    ///     Validates that a combination of culture and segment is valid for the content type properties.
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="wildcards">A value indicating whether wildcard are supported.</param>
    /// <returns>True if the combination is valid; otherwise false.</returns>
    /// <remarks>
    ///     <para>
    ///         The combination must be valid for properties of the content type. For instance, if the content type varies by
    ///         culture,
    ///         then an invariant culture is valid, because some properties may be invariant. On the other hand, if the content
    ///         type is invariant,
    ///         then a variant culture is invalid, because no property could possibly vary by culture.
    ///     </para>
    /// </remarks>
    bool SupportsPropertyVariation(string? culture, string segment, bool wildcards = false);
}
