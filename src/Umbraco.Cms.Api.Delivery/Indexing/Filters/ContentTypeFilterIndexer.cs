using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Indexing.Filters;

public sealed class ContentTypeFilterIndexer : IContentIndexHandler
{
    internal const string FieldName = "contentType";

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content)
        => new[] { new IndexFieldValue { FieldName = FieldName, Value = content.ContentType.Alias } };

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.String } };
}
