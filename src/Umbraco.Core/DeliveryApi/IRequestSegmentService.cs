namespace Umbraco.Cms.Core.DeliveryApi;

public interface IRequestSegmmentService
{
    /// <summary>
    ///     Gets the requested segment from the "Accept-Segment" header, if present.
    /// </summary>
    string? GetRequestedSegment();
}
