namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Specifies which "composed of" relationships to include when resolving the content types that build upon a
///     given content type.
/// </summary>
/// <remarks>
///     Inheritance and composition are stored in the same underlying collection and are distinguished only by whether
///     the referencing content type's tree parent is the content type in question. This lets callers ask for a single
///     axis or both.
/// </remarks>
public enum ComposedOfType
{
    /// <summary>
    ///     Content types that use the content type as a true composition (i.e. not tree inheritance).
    /// </summary>
    Composition,

    /// <summary>
    ///     Content types that inherit from the content type in the tree.
    /// </summary>
    Inheritance,

    /// <summary>
    ///     Both composition users and inheritance children.
    /// </summary>
    All,
}
