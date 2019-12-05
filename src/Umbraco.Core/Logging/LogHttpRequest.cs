using System;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Logging
{
    public static class LogHttpRequest
    {
        static readonly string RequestIdItemName = typeof(LogHttpRequest).Name + "+RequestId";

        /// <summary>
        /// Retrieve the id assigned to the currently-executing HTTP request, if any.
        /// </summary>
        /// <param name="requestId">The request id.</param>
        /// <returns><c>true</c> if there is a request in progress; <c>false</c> otherwise.</returns>
        public static bool TryGetCurrentHttpRequestId(out Guid requestId)
        {
            var requestIdItem = Current.AppCaches.RequestCache.Get(RequestIdItemName, () => Guid.NewGuid());
            requestId = (Guid)requestIdItem;

            return true;
        }
    }
}
