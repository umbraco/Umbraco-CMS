namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// Thrown from the imaging library's decode hook when a request waited without getting a place
/// within the image processing concurrency limit.
/// </summary>
/// <remarks>
/// The hook has no way of saying "do not decode this" other than to throw - its return value only
/// augments the decoder options. The imaging middleware logs and rethrows, so this reaches
/// <see cref="ImageProcessingThrottleMiddleware" />, which turns it into a response.
/// </remarks>
internal sealed class ImageProcessingUnavailableException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageProcessingUnavailableException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ImageProcessingUnavailableException(string message)
        : base(message)
    {
    }
}
