using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Indexing.Sorts;

public sealed class UpdateDateSortIndexer : IContentIndexHandler
{
    internal const string FieldName = "updated";

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content)
        => new[] { new IndexFieldValue { FieldName = FieldName, Value = content.UpdateDate } };

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.Date } };
}
