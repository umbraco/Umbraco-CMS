using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Indexing.Sorts;

public sealed class NameSortIndexer : IContentIndexHandler
{
    internal const string FieldName = "name";

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content, string? culture)
        => new[] { new IndexFieldValue { FieldName = FieldName, Values = new object[] { content.GetCultureName(culture) ?? string.Empty } } };

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.StringSortable, VariesByCulture = true } };
}
