namespace Umbraco.Cms.Core.DeliveryApi;

public class SortOption
{
    public required string FieldName { get; set; }

    public required Direction Direction { get; set; }
}
