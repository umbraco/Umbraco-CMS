using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Api.Delivery.Indexing.Selectors;

public sealed class AncestorsSelectorIndexer : IContentIndexHandler
{
    // NOTE: "id" is a reserved field name
    internal const string FieldName = UmbracoExamineFieldNames.DeliveryApiContentIndex.ItemId;

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content, string? culture)
        => new[] { new IndexFieldValue { FieldName = FieldName, Values = new object[] { content.Key } } };

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.StringRaw, VariesByCulture = false } };
}

