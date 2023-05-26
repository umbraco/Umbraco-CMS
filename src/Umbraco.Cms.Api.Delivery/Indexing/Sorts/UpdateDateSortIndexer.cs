using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Indexing.Sorts;

public sealed class UpdateDateSortIndexer : IContentIndexHandler
{
    internal const string FieldName = "updateDate";

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content, string? culture)
        => new[]
        {
            new IndexFieldValue
            {
                FieldName = FieldName,
                Values = new object[] { (culture is not null ? content.GetUpdateDate(culture) : null) ?? content.UpdateDate }
            }
        };

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.Date, VariesByCulture = true } };
}
