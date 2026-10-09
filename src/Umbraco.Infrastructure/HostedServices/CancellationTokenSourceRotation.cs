// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
/// Helpers for rotating <see cref="CancellationTokenSource" /> instances used to interrupt cooperative waits.
/// </summary>
internal static class CancellationTokenSourceRotation
{
    /// <summary>
    /// Atomically installs a fresh <see cref="CancellationTokenSource" /> at <paramref name="field" /> and cancels the previous one without disposing it.
    /// If the previous CTS has already been disposed (lost the shutdown race), the newly installed CTS is also disposed since no waiter will observe it.
    /// </summary>
    /// <param name="field">A reference to the field holding the active CTS.</param>
    /// <remarks>
    /// The previous CTS is not disposed because the wait loop may still be registering against its token. Once cancelled it is small and will be collected by the GC.
    /// </remarks>
    public static void RotateAndCancel(ref CancellationTokenSource field)
    {
        var newCts = new CancellationTokenSource();
        CancellationTokenSource oldCts = Interlocked.Exchange(ref field, newCts);

        try
        {
            oldCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            newCts.Dispose();
        }
    }
}
