namespace Umbraco.Core.Migrations
{
    // exists so the builder can be mocked in tests
    public interface IMigrationCollectionBuilder
    {
        MigrationCollection CreateCollection(IMigrationContext context);
    }
}
