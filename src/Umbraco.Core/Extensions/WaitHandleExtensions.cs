// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Extensions;

public static class WaitHandleExtensions
{
    // http://stackoverflow.com/questions/25382583/waiting-on-a-named-semaphore-with-waitone100-vs-waitone0-task-delay100
    // http://blog.nerdbank.net/2011/07/c-await-for-waithandle.html
    // F# has a AwaitWaitHandle method that accepts a time out... and seems pretty complex...
    // version below should be OK
    public static Task WaitOneAsync(this WaitHandle handle, int millisecondsTimeout = Timeout.Infinite)
    {
        var tcs = new TaskCompletionSource<object?>();
        var callbackHandleInitLock = new object();
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
