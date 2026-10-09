namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// One request's claim on the image processing concurrency limit.
/// </summary>
/// <remarks>
/// Taking the claim and giving it back are deliberately separated. It is taken at the last point
/// before a decode the package can observe, and given back from the imaging middleware's processed
/// hook, the first point after the decoded image is disposed, so a place is held for the decode
/// alone. <see cref="ImageProcessingThrottleMiddleware" /> gives it back again when the request
/// ends, which does nothing after the first release, so it is returned even if the decode faults
/// before reaching the hook. A slot belongs to a single request and is only ever used from within
/// it.
/// </remarks>
internal sealed class ImageProcessingSlot
{
    /// <summary>
    /// The <see cref="Microsoft.AspNetCore.Http.HttpContext.Items" /> key the slot is published under.
    /// </summary>
    internal const string HttpContextItemKey = "Umbraco.Cms.Imaging.ImageSharp.ImageProcessingSlot";

    private readonly SemaphoreSlim _semaphore;
    private readonly TimeSpan _waitTimeout;
    private bool _held;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageProcessingSlot" /> class.
    /// </summary>
    /// <param name="semaphore">The semaphore bounding concurrent processing.</param>
    /// <param name="waitTimeout">How long to wait for a place before giving up on one.</param>
    public ImageProcessingSlot(SemaphoreSlim semaphore, TimeSpan waitTimeout)
    {
        _semaphore = semaphore;
        _waitTimeout = waitTimeout;
    }

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

        if (await _semaphore.WaitAsync(_waitTimeout, cancellationToken) is false)
        {
            throw new ImageProcessingUnavailableException(
                $"Waited {_waitTimeout.TotalSeconds} seconds without a place within the image processing concurrency limit.");
        }

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
