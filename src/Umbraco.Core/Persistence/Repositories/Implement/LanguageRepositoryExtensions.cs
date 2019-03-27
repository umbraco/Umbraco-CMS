namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal static class LanguageRepositoryExtensions
    {
        public static bool IsDefault(this ILanguageRepository repo, string culture)
        {
            return repo.GetDefaultIsoCode().InvariantEquals(culture);
        }
    }
}