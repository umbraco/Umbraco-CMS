namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class FilterOption
{
    public required string FieldName { get; set; }

    public required string Value { get; set; }

    public FilterOperation Operator { get; set; }
}
