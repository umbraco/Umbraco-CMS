namespace Umbraco.Cms.Core.Models.Entities;

public interface IPublishableContentEntitySlim : IContentEntitySlim
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
