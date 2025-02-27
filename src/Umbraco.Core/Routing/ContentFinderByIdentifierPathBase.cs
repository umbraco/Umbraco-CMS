using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Routing;

public abstract class ContentFinderByIdentifierPathBase
{
    private readonly IRequestAccessor _requestAccessor;
    private readonly ILogger<ContentFinderByIdentifierPathBase> _logger;

    /// <remark>
    ///     Used as the log message inside <see cref="LogAndReturnFailure()"/>>.
    /// </remark>
    protected abstract string FailureLogMessageTemplate { get; }

    protected ContentFinderByIdentifierPathBase(IRequestAccessor requestAccessor, ILogger<ContentFinderByIdentifierPathBase> logger)
    {
        _requestAccessor = requestAccessor;
        _logger = logger;
    }

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

    protected Task<bool> LogAndReturnFailure()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(FailureLogMessageTemplate);
        }

        return Task.FromResult(false);
    }
}
