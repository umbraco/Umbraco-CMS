namespace Umbraco.Cms.Api.Management.ViewModels.Content;

internal interface IHasCultureAndSegment
{
    public string? Culture { get; }

    public string? Segment { get; }
}
