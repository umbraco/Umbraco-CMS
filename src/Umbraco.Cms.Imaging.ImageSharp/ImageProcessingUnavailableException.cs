namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// Thrown by <see cref="ImageProcessingSlot" /> when a request waited without getting a place
/// within the image processing concurrency limit.
/// </summary>
/// <remarks>
/// An exception rather than a result because the slot may be taken from the imaging library's
/// decode hook, which has no way of saying "do not decode this" other than to throw - its return
/// value only augments the decoder options. The imaging middleware logs and rethrows, so this
/// reaches <see cref="ImageProcessingThrottleMiddleware" />, which turns it into a response.
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
