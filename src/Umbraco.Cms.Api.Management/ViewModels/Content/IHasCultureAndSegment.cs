namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public interface IHasCultureAndSegment
{
    public string? Culture { get; }

    public string? Segment { get; }
}
