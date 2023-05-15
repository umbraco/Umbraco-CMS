using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Indexing.Sorts;

public sealed class CreateDateSortIndexer : IContentIndexHandler
{
    internal const string FieldName = "createDate";

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content, string? culture)
        => new[] { new IndexFieldValue { FieldName = FieldName, Values = new object[] { content.CreateDate } } };

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.Date, VariesByCulture = false } };
}
