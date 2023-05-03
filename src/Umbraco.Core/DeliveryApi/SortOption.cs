namespace Umbraco.Cms.Core.DeliveryApi;

public class SortOption
{
    public required string FieldName { get; set; }

    public Direction Direction { get; set; }

    public FieldType FieldType { get; set; }
}
