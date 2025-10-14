using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ILanguageService"/>.
/// </summary>
public static class LanguageServiceExtensions
{
    /// <summary>
    /// Retrieves all ISO codes of all languages.
    /// </summary>
    /// <param name="service">The <see cref="ILanguageService"/>.</param>
    /// <returns>A collection of language ISO codes.</returns>
    public static async Task<IEnumerable<string>> GetAllIsoCodesAsync(this ILanguageService service) => (await service.GetAllAsync()).Select(x => x.IsoCode);
}
