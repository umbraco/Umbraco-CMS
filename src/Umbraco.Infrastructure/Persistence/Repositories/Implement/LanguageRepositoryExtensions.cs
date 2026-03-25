using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal static class LanguageRepositoryExtensions
{
    public static async Task<bool> IsDefaultAsync(this ILanguageRepository repo, string? culture)
    {
        if (culture == null || culture == "*")
        {
            return false;
        }

        var defaultIso = await repo.GetDefaultIsoCodeAsync();

        return defaultIso.InvariantEquals(culture);
    }
}
