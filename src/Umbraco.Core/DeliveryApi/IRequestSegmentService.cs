namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a service that retrieves the requested segment from the current HTTP request.
/// </summary>
public interface IRequestSegmentService
{
    /// <summary>
    ///     Gets the requested segment from the "Accept-Segment" header, if present.
    /// </summary>
    string? GetRequestedSegment();
}
