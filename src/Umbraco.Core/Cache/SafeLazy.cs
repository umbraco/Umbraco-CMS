using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Umbraco.Cms.Core.Cache
{
    public static class SafeLazy
    {
        // an object that represent a value that has not been created yet
#pragma warning disable IDE1006 // Naming Styles
        internal static readonly object ValueNotCreated = new object();
#pragma warning restore IDE1006 // Naming Styles

        public static Lazy<object?> GetSafeLazy(Func<object?> getCacheItem)
        {
            // try to generate the value and if it fails,
            // wrap in an ExceptionHolder - would be much simpler
            // to just use lazy.IsValueFaulted alas that field is
            // internal
            return new Lazy<object?>(() =>
            {
                try
                {
                    return getCacheItem();
                }
                catch (Exception e)
                {
                    return new ExceptionHolder(ExceptionDispatchInfo.Capture(e));
                }
            });
        }

        public static object? GetSafeLazyValue(Lazy<object?>? lazy, bool onlyIfValueIsCreated = false)
        {
            // if onlyIfValueIsCreated, do not trigger value creation
            // must return something, though, to differentiate from null values
            if (onlyIfValueIsCreated && lazy?.IsValueCreated == false) return ValueNotCreated;

            // if execution has thrown then lazy.IsValueCreated is false
            // and lazy.IsValueFaulted is true (but internal) so we use our
            // own exception holder (see Lazy<T> source code) to return null
            if (lazy?.Value is ExceptionHolder) return null;

            // we have a value and execution has not thrown so returning
            // here does not throw - unless we're re-entering, take care of it
            try
            {
                return lazy?.Value;
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException("The method that computes a value for the cache has tried to read that value from the cache.", e);
            }
        }

        public class ExceptionHolder
        {
            public ExceptionHolder(ExceptionDispatchInfo e)
            {
                Exception = e;
            }

            public ExceptionDispatchInfo Exception { get; }
        }
    }
    //https://devblogs.microsoft.com/pfxteam/asynclazyt/
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) :
            base(() => Task.Factory.StartNew(valueFactory))
        { }

        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap())
        { }
        public TaskAwaiter<T> GetAwaiter() { return Value.GetAwaiter(); }
    }

    public static class AsyncSafeLazy
    {
        // an object that represent a value that has not been created yet
#pragma warning disable IDE1006 // Naming Styles
        internal static readonly object ValueNotCreated = new object();
#pragma warning restore IDE1006 // Naming Styles

        public static AsyncLazy<object?> GetAsyncSafeLazy(Func<Task<object?>> getCacheItemAsync)
        {
            // try to generate the value and if it fails,
            // wrap in an ExceptionHolder - would be much simpler
            // to just use lazy.IsValueFaulted alas that field is
            // internal
            return new AsyncLazy<object?>(async () =>
            {
                try
                {
                    return await getCacheItemAsync();
                }
                catch (Exception e)
                {
                    return new ExceptionHolder(ExceptionDispatchInfo.Capture(e));
                }
            });
        }

        public static object? GetAsyncSafeLazyValue(AsyncLazy<object?>? lazy, bool onlyIfValueIsCreated = false)
        {
            // if onlyIfValueIsCreated, do not trigger value creation
            // must return something, though, to differentiate from null values
            if (onlyIfValueIsCreated && lazy?.IsValueCreated == false)
                return ValueNotCreated;

            // if execution has thrown then lazy.IsValueCreated is false
            // and lazy.IsValueFaulted is true (but internal) so we use our
            // own exception holder (see Lazy<T> source code) to return null
            if (lazy?.Value is ExceptionHolder)
                return null;

            // we have a value and execution has not thrown so returning
            // here does not throw - unless we're re-entering, take care of it
            try
            {
                return lazy?.Value;
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException("The method that computes a value for the cache has tried to read that value from the cache.", e);
            }
        }

        public class ExceptionHolder
        {
            public ExceptionHolder(ExceptionDispatchInfo e)
            {
                Exception = e;
            }

            public ExceptionDispatchInfo Exception { get; }
        }
    }
}
