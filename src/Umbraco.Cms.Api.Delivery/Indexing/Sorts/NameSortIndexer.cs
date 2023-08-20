using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Indexing.Sorts;

public sealed class NameSortIndexer : IContentIndexHandler
{
    internal const string FieldName = "sortName";

    public IEnumerable<IndexFieldValue> GetFieldValues(IContentBase content, string? culture)
        => new[] { new IndexFieldValue { FieldName = FieldName, Values = new object[] { content.GetCultureName(culture) ?? string.Empty } } };

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.StringSortable, VariesByCulture = true } };
}
