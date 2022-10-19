// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

public static class DelegateExtensions
{
    public static Attempt<T?> RetryUntilSuccessOrTimeout<T>(this Func<Attempt<T?>> task, TimeSpan timeout, TimeSpan pause)
    {
        if (pause.TotalMilliseconds < 0)
        {
            throw new ArgumentException("pause must be >= 0 milliseconds");
        }

        var stopwatch = Stopwatch.StartNew();
        do
        {
            Attempt<T?> result = task();
            if (result.Success)
            {
                return result;
            }

            Thread.Sleep((int)pause.TotalMilliseconds);
        }
        while (stopwatch.Elapsed < timeout);

        return Attempt<T?>.Fail();
    }

    public static Attempt<T?> RetryUntilSuccessOrMaxAttempts<T>(this Func<int, Attempt<T?>> task, int totalAttempts, TimeSpan pause)
    {
        if (pause.TotalMilliseconds < 0)
        {
            throw new ArgumentException("pause must be >= 0 milliseconds");
        }

        var attempts = 0;
        do
        {
            attempts++;
            Attempt<T?> result = task(attempts);
            if (result.Success)
            {
                return result;
            }

            Thread.Sleep((int)pause.TotalMilliseconds);
        }
        while (attempts < totalAttempts);

        return Attempt<T?>.Fail();
    }
}
