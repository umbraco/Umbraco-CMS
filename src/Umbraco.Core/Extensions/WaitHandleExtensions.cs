// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for <see cref="WaitHandle"/>.
/// </summary>
public static class WaitHandleExtensions
{
    /// <summary>
    /// Asynchronously waits for the wait handle to be signaled.
    /// </summary>
    /// <param name="handle">The wait handle to wait on.</param>
    /// <param name="millisecondsTimeout">The timeout in milliseconds. Defaults to <see cref="Timeout.Infinite"/>.</param>
    /// <returns>A task that completes when the wait handle is signaled or the timeout elapses.</returns>
    /// <remarks>
    /// Based on http://stackoverflow.com/questions/25382583/waiting-on-a-named-semaphore-with-waitone100-vs-waitone0-task-delay100
    /// and http://blog.nerdbank.net/2011/07/c-await-for-waithandle.html.
    /// </remarks>
    public static Task WaitOneAsync(this WaitHandle handle, int millisecondsTimeout = Timeout.Infinite)
    {
        var tcs = new TaskCompletionSource<object?>();
        var callbackHandleInitLock = new Lock();
        lock (callbackHandleInitLock)
        {
            RegisteredWaitHandle? callbackHandle = null;

            // ReSharper disable once RedundantAssignment
            callbackHandle = ThreadPool.RegisterWaitForSingleObject(
                handle,
                (state, timedOut) =>
                {
                    // TODO: We aren't checking if this is timed out
                    tcs.SetResult(null);

                    // we take a lock here to make sure the outer method has completed setting the local variable callbackHandle.
                    lock (callbackHandleInitLock)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        // ReSharper disable once AccessToModifiedClosure
                        callbackHandle?.Unregister(null);
                    }
                },
                /*state:*/ null,
                /*millisecondsTimeOutInterval:*/ millisecondsTimeout,
                /*executeOnlyOnce:*/ true);
        }

        return tcs.Task;
    }
}
