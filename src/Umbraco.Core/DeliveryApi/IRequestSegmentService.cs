namespace Umbraco.Cms.Core.DeliveryApi;

public interface IRequestSegmentService
{
    /// <summary>
    ///     Gets the requested segment from the "Accept-Segment" header, if present.
    /// </summary>
    string? GetRequestedSegment();
}

[Obsolete("This interface is misspelled and will be removed in Umbraco 18. Please use the correct one IRequestSegmentService")]
public interface IRequestSegmmentService
{
    /// <summary>
    ///     Gets the requested segment from the "Accept-Segment" header, if present.
    /// </summary>
    string? GetRequestedSegment();
}
