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
///     This interface is misspelled and will be removed in Umbraco 18. Please use the correct one <see cref="IRequestSegmentService"/>.
/// </remarks>
[Obsolete("This interface is misspelled and will be removed in Umbraco 18. Please use the correct one IRequestSegmentService")]
public interface IRequestSegmmentService
{
    /// <summary>
    ///     Gets the requested segment from the "Accept-Segment" header, if present.
    /// </summary>
    string? GetRequestedSegment();
}
