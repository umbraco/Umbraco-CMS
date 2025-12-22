namespace Umbraco.Cms.Web.Common.Exceptions;

/// <summary>
///     Exception that occurs when an Umbraco form route string is invalid
/// </summary>
public sealed class HttpUmbracoFormRouteStringException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpUmbracoFormRouteStringException" /> class.
    /// </summary>
    public HttpUmbracoFormRouteStringException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpUmbracoFormRouteStringException" /> class.
    /// </summary>
    /// <param name="message">The error message displayed to the client when the exception is thrown.</param>
    public HttpUmbracoFormRouteStringException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpUmbracoFormRouteStringException" /> class.
    /// </summary>
    /// <param name="message">The error message displayed to the client when the exception is thrown.</param>
    /// <param name="innerException">
    ///     The <see cref="P:System.Exception.InnerException" />, if any, that threw the current
    ///     exception.
    /// </param>
    public HttpUmbracoFormRouteStringException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
