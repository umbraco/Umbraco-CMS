namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Represents a lightweight document entity, managed by the entity service.
/// </summary>
public interface IDocumentEntitySlim : IContentEntitySlim
{
    /// <summary>
    ///     Gets the variant name for each culture
    /// </summary>
    IReadOnlyDictionary<string, string> CultureNames { get; }

    /// <summary>
    ///     Gets the published cultures.
    /// </summary>
    IEnumerable<string> PublishedCultures { get; }

    /// <summary>
    ///     Gets the edited cultures.
    /// </summary>
    IEnumerable<string> EditedCultures { get; }

    /// <summary>
    ///     Gets the content variation of the content type.
    /// </summary>
    ContentVariation Variations { get; }

    /// <summary>
    ///     Gets a value indicating whether the content is published.
    /// </summary>
    bool Published { get; }

    /// <summary>
    ///     Gets a value indicating whether the content has been edited.
    /// </summary>
    bool Edited { get; }
}
