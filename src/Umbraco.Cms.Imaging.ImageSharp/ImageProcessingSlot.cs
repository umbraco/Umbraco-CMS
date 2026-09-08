namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// One request's claim on the image processing concurrency limit.
/// </summary>
/// <remarks>
/// Taking the claim and giving it back are deliberately separated. It is taken from the imaging
/// middleware's decode hook, which is the first point at which a request is known to be decoding
/// rather than being served from cache, and given back by
/// <see cref="ImageProcessingThrottleMiddleware" /> when the request ends, so it is returned even
/// if the decode faults. A slot belongs to a single request and is only ever used from within it.
/// </remarks>
internal sealed class ImageProcessingSlot
{
    /// <summary>
    /// The <see cref="Microsoft.AspNetCore.Http.HttpContext.Items" /> key the slot is published under.
    /// </summary>
    internal const string HttpContextItemKey = "Umbraco.Cms.Imaging.ImageSharp.ImageProcessingSlot";

    private readonly SemaphoreSlim _semaphore;
    private bool _held;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageProcessingSlot" /> class.
    /// </summary>
    /// <param name="semaphore">The semaphore bounding concurrent processing.</param>
    public ImageProcessingSlot(SemaphoreSlim semaphore) => _semaphore = semaphore;

    /// <summary>
    /// Waits for a place within the concurrency limit, unless this request already holds one.
    /// </summary>
    /// <param name="cancellationToken">A token to abandon the wait.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task AcquireAsync(CancellationToken cancellationToken)
    {
        // The imaging middleware retries a request whose cached result was removed underneath it,
        // running the decode hook a second time. Waiting again would take a place the request has
        // no way to give back.
        if (_held)
        {
            return;
        }

        await _semaphore.WaitAsync(cancellationToken);
        _held = true;
    }

    /// <summary>
    /// Gives back the place held within the concurrency limit, if one was ever taken.
    /// </summary>
    public void Release()
    {
        if (_held is false)
        {
            return;
        }

        _held = false;
        _semaphore.Release();
    }
}
