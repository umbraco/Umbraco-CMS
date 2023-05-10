namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class FilterOption
{
    public required string FieldName { get; set; }

    public required string[] Values { get; set; }

    public required FilterOperation Operator { get; set; }
}
