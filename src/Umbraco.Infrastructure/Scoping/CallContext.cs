using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Provides a way to set contextual data that flows with the call and
    /// async context of a test or invocation.
    /// </summary>
    public static class CallContext<T>
    {
        static ConcurrentDictionary<string, AsyncLocal<T>> _state = new ConcurrentDictionary<string, AsyncLocal<T>>();

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param>
        public static void SetData(string name, T data) => _state.GetOrAdd(name, _ => new AsyncLocal<T>()).Value = data;

        /// <summary>
        /// Retrieves an object with the specified name from the <see cref="CallContext{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data being retrieved. Must match the type used when the <paramref name="name"/> was set via <see cref="SetData{T}(string, T)"/>.</typeparam>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or a default value for <typeparamref name="T"/> if none is found.</returns>
        public static T GetData(string name) => _state.TryGetValue(name, out var data) ? data.Value : default;

        /// <summary>
        /// Clears the state from <see cref="CallContext{T}"/> for the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool RemoveData(string name) => _state.TryRemove(name, out _);
    }
}
