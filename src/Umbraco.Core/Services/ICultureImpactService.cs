using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface ICultureImpactService
{
    /// <summary>
    /// Creates an impact instance representing the impact of a culture set,
    /// in the context of a content item variation.
    /// </summary>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDefault">A value indicating whether the culture is the default culture.</param>
    /// <param name="content">The content item.</param>
    /// <remarks>
    /// <para>Validates that the culture is compatible with the variation.</para>
    /// </remarks>
    CultureImpact? Create(string culture, bool isDefault, IContent content);

    /// <summary>
    /// Gets the impact of 'all' cultures (including the invariant culture).
    /// </summary>
    CultureImpact CreateImpactAll();

    /// <summary>
    /// Gets the impact of the invariant culture.
    /// </summary>
    CultureImpact CreateInvariant();

    /// <summary>
    /// Creates an impact instance representing the impact of a specific culture.
    /// </summary>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDefault">A value indicating whether the culture is the default culture.</param>
    CultureImpact CreateExplicit(string? culture, bool isDefault);

    /// <summary>
    /// Utility method to return the culture used for invariant property errors based on what cultures are being actively saved,
    /// the default culture and the state of the current content item
    /// </summary>
    string? GetCultureForInvariantErrors(IContent? content, string?[] savingCultures, string? defaultCulture);
}
