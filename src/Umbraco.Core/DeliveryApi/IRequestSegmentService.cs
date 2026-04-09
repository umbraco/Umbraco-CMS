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

/// <summary>
///     Defines a service that retrieves the requested segment from the current HTTP request.
/// </summary>
/// <remarks>
///     This interface is misspelled. Please use the correct one <see cref="IRequestSegmentService"/>. Scheduled for removal in Umbraco 18.
/// </remarks>
[Obsolete("This interface is misspelled. Please use the correct one IRequestSegmentService. Scheduled for removal in Umbraco 18.")]
public interface IRequestSegmmentService
{
    /// <summary>
    ///     Gets the requested segment from the "Accept-Segment" header, if present.
    /// </summary>
    string? GetRequestedSegment();
}
