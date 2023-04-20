using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Indexing.Sorts;

internal sealed class SortOrderSortIndexer : IContentIndexHandler
{
    internal const string FieldName = "sortOrder";

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content)
        => new[] { new IndexFieldValue { FieldName = FieldName, Value = content.SortOrder } };

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.Number } };
}
