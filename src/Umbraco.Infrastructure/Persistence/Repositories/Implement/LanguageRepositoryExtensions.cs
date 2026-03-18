using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal static class LanguageRepositoryExtensions
{
    /// <summary>
    /// Determines whether the specified culture is the default language.
    /// </summary>
    /// <param name="repo">The language repository.</param>
    /// <param name="culture">The culture code to check.</param>
    /// <returns>True if the culture is the default language; otherwise, false.</returns>
    public static bool IsDefault(this ILanguageRepository repo, string? culture)
    {
        if (culture == null || culture == "*")
        {
            return false;
        }

        return repo.GetDefaultIsoCode().InvariantEquals(culture);
    }
}
