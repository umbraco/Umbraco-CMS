namespace Umbraco.Cms.Infrastructure.Persistence.EFCore;

/// <summary>
/// Provides extension methods for string operations related to EF Core persistence.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Compares two database provider names, handling case variations for SQLite provider names.
    /// </summary>
    /// <param name="connectionProvider">The connection provider name to compare.</param>
    /// <param name="compareString">The string to compare against.</param>
    /// <returns><c>true</c> if the provider names match; otherwise, <c>false</c>.</returns>
    internal static bool CompareProviderNames(this string connectionProvider, string? compareString)
    {
        if (compareString is null)
        {
            return false;
        }

        if (connectionProvider == compareString)
        {
            return true;
        }

        return connectionProvider is "Microsoft.Data.SQLite" or Constants.ProviderNames.SQLLite && compareString is "Microsoft.Data.SQLite" or Constants.ProviderNames.SQLLite;
    }
}
