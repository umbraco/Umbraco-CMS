using System;
using System.Diagnostics;
using System.Threading;

namespace Umbraco.Core
{
    public static class DelegateExtensions
    {
        public static Attempt<T> RetryUntilSuccessOrTimeout<T>(this Func<Attempt<T>> task, TimeSpan timeout, TimeSpan pause)
        {
            if (pause.TotalMilliseconds < 0)
            {
                throw new ArgumentException("pause must be >= 0 milliseconds");
            }
            var stopwatch = Stopwatch.StartNew();
            do
            {
                var result = task();
                if (result) { return result; }
                Thread.Sleep((int)pause.TotalMilliseconds);
            }
            while (stopwatch.Elapsed < timeout);
            return Attempt<T>.Fail();
        }

        public static Attempt<T> RetryUntilSuccessOrMaxAttempts<T>(this Func<int, Attempt<T>> task, int totalAttempts, TimeSpan pause)
        {
            if (pause.TotalMilliseconds < 0)
            {
                throw new ArgumentException("pause must be >= 0 milliseconds");
            }
            int attempts = 0;
            do
            {
                attempts++;
                var result = task(attempts);
                if (result) { return result; }
                Thread.Sleep((int)pause.TotalMilliseconds);
            }
            while (attempts < totalAttempts);
            return Attempt<T>.Fail();
        }
    }
}