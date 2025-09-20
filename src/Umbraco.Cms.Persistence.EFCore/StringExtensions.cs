using Umbraco.Cms.Core;

namespace Umbraco.Cms.Persistence.EFCore;

internal static class StringExtensions
{
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
