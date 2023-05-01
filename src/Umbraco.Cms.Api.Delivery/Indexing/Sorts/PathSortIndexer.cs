using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Indexing.Sorts;

public sealed class PathSortIndexer : IContentIndexHandler
{
    internal const string FieldName = "path";

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content, string? culture)
        => new[] { new IndexFieldValue { FieldName = FieldName, Values = new object[] { content.Path } } };

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.StringSortable, VariesByCulture = false } };
}
