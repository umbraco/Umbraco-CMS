namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class SelectorOption
{
    public required string FieldName { get; set; }

    public required string[] Values { get; set; }
}
