namespace Umbraco.Cms.Core.Models.ContentEditing;

public interface IHasCultureAndSegment
{
    public string? Culture { get; }

    public string? Segment { get; }
}
