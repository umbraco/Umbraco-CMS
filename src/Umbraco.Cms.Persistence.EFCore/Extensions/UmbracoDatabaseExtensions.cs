namespace Umbraco.Cms.Infrastructure.Persistence;

internal static class UmbracoDatabaseExtensions
{
    [Obsolete("This will be removed when NPOCO is removed from the Umbraco repositories")]
    public static UmbracoDatabase AsUmbracoDatabase(this IUmbracoDatabase database)
    {
        if (database is Cms.Persistence.EFCore.Databases.UmbracoDatabase efDatabase)
        {
            return efDatabase.LegacyUmbracoDatabase;
        }

        if (database is not UmbracoDatabase asDatabase)
        {
            throw new Exception("oops: database.");
        }

        return asDatabase;
    }
}
