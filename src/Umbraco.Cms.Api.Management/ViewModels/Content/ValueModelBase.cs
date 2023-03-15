namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ValueModelBase : IHasCultureAndSegment
{
    public string? Culture { get; set; }

    public string? Segment { get; set; }

    public string Alias { get; set; } = string.Empty;

    public object? Value { get; set; }
}
