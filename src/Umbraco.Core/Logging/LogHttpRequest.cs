using System;
using System.Web;

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
            if (HttpContext.Current == null)
            {
                requestId = default(Guid);
                return false;
            }

            var requestIdItem = HttpContext.Current.Items[RequestIdItemName];
            if (requestIdItem == null)
                HttpContext.Current.Items[RequestIdItemName] = requestId = Guid.NewGuid();
            else
                requestId = (Guid)requestIdItem;

            return true;
        }
    }
}
