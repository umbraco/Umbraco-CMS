// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Imaging.ImageSharp;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Imaging.ImageSharp;

[TestFixture]
public class ImageProcessingSlotTests
{
    // Long enough that a place already free is taken without fuss, short enough that the test
    // asserting the timeout does not sit here.
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromMilliseconds(250);

    [Test]
    public async Task AcquireAsync_TakesOnePlace()
    {
        using var semaphore = new SemaphoreSlim(2, 2);
        var slot = new ImageProcessingSlot(semaphore, WaitTimeout);

        await slot.AcquireAsync(CancellationToken.None);

        Assert.That(semaphore.CurrentCount, Is.EqualTo(1));
    }

    // The imaging middleware runs the decode hook again when it retries a request, and a second
    // place is one the request has no way to give back.
    [Test]
    public async Task AcquireAsync_WhenAlreadyHeld_TakesNoFurtherPlace()
    {
        using var semaphore = new SemaphoreSlim(2, 2);
        var slot = new ImageProcessingSlot(semaphore, WaitTimeout);

        await slot.AcquireAsync(CancellationToken.None);
        await slot.AcquireAsync(CancellationToken.None);

        Assert.That(semaphore.CurrentCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Release_GivesThePlaceBack()
    {
        using var semaphore = new SemaphoreSlim(1, 1);
        var slot = new ImageProcessingSlot(semaphore, WaitTimeout);

        await slot.AcquireAsync(CancellationToken.None);
        slot.Release();

        Assert.That(semaphore.CurrentCount, Is.EqualTo(1));
    }

    // The middleware releases at the end of every gated request, including the ones the imaging
    // middleware served from cache without ever taking a place.
    [Test]
    public void Release_WhenNeverAcquired_DoesNothing()
    {
        using var semaphore = new SemaphoreSlim(1, 1);
        var slot = new ImageProcessingSlot(semaphore, WaitTimeout);

        Assert.DoesNotThrow(slot.Release);
        Assert.That(semaphore.CurrentCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Release_WhenCalledTwice_GivesBackOnlyOnePlace()
    {
        using var semaphore = new SemaphoreSlim(1, 1);
        var slot = new ImageProcessingSlot(semaphore, WaitTimeout);

        await slot.AcquireAsync(CancellationToken.None);

        slot.Release();

        // Releasing beyond the initial count would otherwise let more through than configured.
        Assert.DoesNotThrow(slot.Release);
        Assert.That(semaphore.CurrentCount, Is.EqualTo(1));
    }

    [Test]
    public void AcquireAsync_WhenCancelled_TakesNoPlace()
    {
        using var semaphore = new SemaphoreSlim(0, 1);
        var slot = new ImageProcessingSlot(semaphore, WaitTimeout);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.CatchAsync<OperationCanceledException>(() => slot.AcquireAsync(cancellation.Token));

        // Nothing was taken, so the later release must not hand back a place that was never held.
        slot.Release();
        Assert.That(semaphore.CurrentCount, Is.Zero);
    }

    [Test]
    public void AcquireAsync_WhenNoPlaceComesFree_GivesUp()
    {
        using var semaphore = new SemaphoreSlim(0, 1);
        var slot = new ImageProcessingSlot(semaphore, WaitTimeout);

        Assert.ThrowsAsync<ImageProcessingUnavailableException>(() => slot.AcquireAsync(CancellationToken.None));

        // Nothing was taken, so the release at the end of the request must hand nothing back.
        slot.Release();
        Assert.That(semaphore.CurrentCount, Is.Zero);
    }
}
