using System.Collections.Generic;

namespace Umbraco.Core.Persistence
{
    public interface IBulkSqlInsertProvider
    {
        int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records);
    }
}
