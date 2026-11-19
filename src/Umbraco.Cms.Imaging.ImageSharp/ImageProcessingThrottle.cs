using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// How long a request waits for a place within the image processing concurrency limit, and what
/// happens when it does not get one.
/// </summary>
/// <remarks>
/// Nothing here is specific to a major version of the imaging library, so this file is compiled into
/// the ImageSharp 2.x package as a linked source file rather than copied.
/// </remarks>
internal static class ImageProcessingThrottle
{
    /// <summary>
    /// How long a request waits for a place within the concurrency limit before it is rejected.
    /// </summary>
    /// <remarks>
    /// Waiting without a bound is not "degrading in throughput", it is hanging: the waiters pile up
    /// holding an open source stream each, and nothing sheds the load. Generous enough that only
    /// pathological demand reaches it - the limit is at least one decode, and a decode is a matter
    /// of milliseconds to a second - and short enough to sit inside the request timeout of a
    /// typical proxy, so the site answers rather than the proxy giving up on it.
    /// </remarks>
    internal static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Turns away a request that waited without getting a place.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <param name="logger">The logger.</param>
    /// <remarks>
    /// A rejection is the sign that demand is beyond what the host's memory can serve, so it is
    /// logged as a warning rather than passed over quietly. <c>503</c> with a <c>Retry-After</c> is
    /// what a proxy or CDN in front of the site knows how to act on.
    /// </remarks>
    internal static void Reject(HttpContext context, ILogger logger)
    {
        logger.LogWarning(
            "Turned away an image processing request for {Path} after waiting {WaitTimeoutSeconds} seconds for a place within the concurrency limit. The demand for image processing is beyond what this host's memory can serve.",
            context.Request.Path.Value,
            (int)WaitTimeout.TotalSeconds);

        // The imaging middleware writes its response after the decode, so this is reached before
        // anything is sent. Guarded because being wrong about that would throw over the top of a
        // response already on its way.
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers.RetryAfter = "5";
    }
}
