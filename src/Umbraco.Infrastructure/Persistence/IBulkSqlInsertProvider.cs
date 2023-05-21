namespace Umbraco.Cms.Infrastructure.Persistence;

public interface IBulkSqlInsertProvider
{
    string ProviderName { get; }

    int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records);
}
