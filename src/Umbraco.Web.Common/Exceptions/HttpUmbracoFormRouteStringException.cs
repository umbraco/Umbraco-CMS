using System.Runtime.Serialization;

namespace Umbraco.Cms.Web.Common.Exceptions;

/// <summary>
///     Exception that occurs when an Umbraco form route string is invalid
/// </summary>
[Serializable]
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpUmbracoFormRouteStringException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
    ///     data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that holds the contextual
    ///     information about the source or destination.
    /// </param>
    private HttpUmbracoFormRouteStringException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
