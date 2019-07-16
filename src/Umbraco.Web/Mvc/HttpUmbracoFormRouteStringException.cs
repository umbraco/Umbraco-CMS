using System;
using System.Net;
using System.Web;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Exception that occurs when an Umbraco form route string is invalid
    /// </summary>
    /// <seealso cref="System.Web.HttpException" />
    [Serializable]
    public sealed class HttpUmbracoFormRouteStringException : HttpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpUmbracoFormRouteStringException" /> class.
        /// </summary>
        /// <param name="message">The error message displayed to the client when the exception is thrown.</param>
        public HttpUmbracoFormRouteStringException(string message)
            : base(message)
        { }

    }
}
