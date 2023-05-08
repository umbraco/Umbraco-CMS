namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class IndexField
{
    public required string FieldName { get; set; }

    public required FieldType FieldType { get; set; }

    public required bool VariesByCulture { get; set; }
}
