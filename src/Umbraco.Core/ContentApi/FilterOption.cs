namespace Umbraco.Cms.Core.ContentApi;

public class FilterOption
{
    public required string FieldName { get; set; }

    public required string Value { get; set; }

    public FilterOperation Operator { get; set; }
}

public enum FilterOperation
{
    Is,
    IsNot,
    // TODO: how to handle these in Examine?
    Contains,
    DoesNotContain
}
