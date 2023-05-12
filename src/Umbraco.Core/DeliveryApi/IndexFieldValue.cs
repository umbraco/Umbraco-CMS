namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class IndexFieldValue
{
    public required string FieldName { get; set; }

    public required IEnumerable<object> Values { get; set; }
}
