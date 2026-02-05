using System.Runtime.ExceptionServices;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides methods for creating and working with lazy values that safely handle exceptions.
/// </summary>
/// <remarks>
///     Safe lazy values capture exceptions during evaluation and return them as null values
///     instead of re-throwing, preventing cache corruption from failed evaluations.
/// </remarks>
public static class SafeLazy
{
    /// <summary>
    ///     An object that represents a value that has not been created yet.
    /// </summary>
    internal static readonly object ValueNotCreated = new();

    /// <summary>
    ///     Creates a safe lazy that catches exceptions during evaluation.
    /// </summary>
    /// <param name="getCacheItem">The factory function to create the cached item.</param>
    /// <returns>A lazy value that wraps exceptions in an <see cref="ExceptionHolder" /> instead of throwing.</returns>
    public static Lazy<object?> GetSafeLazy(Func<object?> getCacheItem) =>

        // try to generate the value and if it fails,
        // wrap in an ExceptionHolder - would be much simpler
        // to just use lazy.IsValueFaulted alas that field is
        // internal
        new Lazy<object?>(() =>
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

    /// <summary>
    ///     Gets the value from a safe lazy, returning null for exceptions.
    /// </summary>
    /// <param name="lazy">The lazy value to evaluate.</param>
    /// <param name="onlyIfValueIsCreated">If <c>true</c>, only returns the value if already created; otherwise returns <see cref="ValueNotCreated" />.</param>
    /// <returns>The lazy value, <see cref="ValueNotCreated" /> if not yet created and <paramref name="onlyIfValueIsCreated" /> is <c>true</c>, or <c>null</c> if an exception occurred.</returns>
    public static object? GetSafeLazyValue(Lazy<object?>? lazy, bool onlyIfValueIsCreated = false)
    {
        // if onlyIfValueIsCreated, do not trigger value creation
        // must return something, though, to differentiate from null values
        if (onlyIfValueIsCreated && lazy?.IsValueCreated == false)
        {
            return ValueNotCreated;
        }

        // if execution has thrown then lazy.IsValueCreated is false
        // and lazy.IsValueFaulted is true (but internal) so we use our
        // own exception holder (see Lazy<T> source code) to return null
        if (lazy?.Value is ExceptionHolder)
        {
            return null;
        }

        // we have a value and execution has not thrown so returning
        // here does not throw - unless we're re-entering, take care of it
        try
        {
            return lazy?.Value;
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException(
                "The method that computes a value for the cache has tried to read that value from the cache.", e);
        }
    }

    /// <summary>
    ///     Holds an exception that occurred during lazy value evaluation.
    /// </summary>
    /// <remarks>
    ///     Used to store exceptions in the cache without re-throwing them during normal access,
    ///     allowing the exception to be thrown once when the value is actually requested.
    /// </remarks>
    public class ExceptionHolder
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExceptionHolder" /> class.
        /// </summary>
        /// <param name="e">The captured exception dispatch info.</param>
        public ExceptionHolder(ExceptionDispatchInfo e) => Exception = e;

        /// <summary>
        ///     Gets the captured exception dispatch info.
        /// </summary>
        public ExceptionDispatchInfo Exception { get; }
    }
}
