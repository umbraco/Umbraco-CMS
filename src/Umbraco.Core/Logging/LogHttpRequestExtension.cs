using Umbraco.Cms.Core.Cache;

namespace Umbraco.Extensions;

public static class LogHttpRequest
{
    private static readonly string RequestIdItemName = typeof(LogHttpRequest).Name + "+RequestId";

    /// <summary>
    ///     Retrieve the id assigned to the currently-executing HTTP request, if any.
    /// </summary>
    /// <param name="requestId">The request id.</param>
    /// <param name="requestCache"></param>
    /// <returns><c>true</c> if there is a request in progress; <c>false</c> otherwise.</returns>
    public static bool TryGetCurrentHttpRequestId(out Guid? requestId, IRequestCache requestCache)
    {
        var requestIdItem = requestCache.Get(RequestIdItemName, () => Guid.NewGuid());
        requestId = (Guid?)requestIdItem;

        return true;
    }
}
