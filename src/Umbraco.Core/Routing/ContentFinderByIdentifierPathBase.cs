using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides a base class for content finders that handle identifier-based paths (like keys or IDs).
/// </summary>
public abstract class ContentFinderByIdentifierPathBase
{
    private readonly IRequestAccessor _requestAccessor;
    private readonly ILogger<ContentFinderByIdentifierPathBase> _logger;

    /// <summary>
    ///     Gets the log message template used when content lookup fails.
    /// </summary>
    /// <remark>
    ///     Used as the log message inside <see cref="LogAndReturnFailure()"/>.
    /// </remark>
    protected abstract string FailureLogMessageTemplate { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByIdentifierPathBase" /> class.
    /// </summary>
    /// <param name="requestAccessor">The request accessor.</param>
    /// <param name="logger">The logger.</param>
    protected ContentFinderByIdentifierPathBase(IRequestAccessor requestAccessor, ILogger<ContentFinderByIdentifierPathBase> logger)
    {
        _requestAccessor = requestAccessor;
        _logger = logger;
    }

    /// <summary>
    ///     Resolves the culture from the query string and sets it on the request.
    /// </summary>
    /// <param name="frequest">The published request builder.</param>
    protected void ResolveAndSetCultureOnRequest(IPublishedRequestBuilder frequest)
    {
        var cultureFromQuerystring = _requestAccessor.GetQueryStringValue("culture");

        // Check if we have a culture in the query string
        if (!string.IsNullOrEmpty(cultureFromQuerystring))
        {
            // We're assuming it will match a culture, if an invalid one is passed in,
            // an exception will throw (there is no TryGetCultureInfo method)
            frequest.SetCulture(cultureFromQuerystring);
        }
    }

    /// <summary>
    ///     Resolves the segment from the query string and sets it on the request.
    /// </summary>
    /// <param name="frequest">The published request builder.</param>
    protected void ResolveAndSetSegmentOnRequest(IPublishedRequestBuilder frequest)
    {
        var segmentFromQuerystring = _requestAccessor.GetQueryStringValue("segment");

        // Check if we have a segment in the query string
        if (!string.IsNullOrEmpty(segmentFromQuerystring))
        {
            // We're assuming it will match a segment
            frequest.SetSegment(segmentFromQuerystring);
        }
    }

    /// <summary>
    ///     Logs a debug message and returns a failed task result.
    /// </summary>
    /// <returns>A task that returns false.</returns>
    protected Task<bool> LogAndReturnFailure()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(FailureLogMessageTemplate);
        }

        return Task.FromResult(false);
    }
}
