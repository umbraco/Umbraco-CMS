using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Provides a way to set contextual data that flows with the call and
    /// async context of a test or invocation.
    /// </summary>
    public static class CallContext
    {
        private static readonly ConcurrentDictionary<string, Guid?> _state = new ConcurrentDictionary<string, Guid?>();

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param>
        public static void SetData(string name, Guid? data)
        {
            _state[name + Thread.CurrentThread.ManagedThreadId] = data;
        }


        /// <summary>
        /// Retrieves an object with the specified name from the <see cref="CallContext"/>.
        /// </summary>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or <see langword="null"/> if not found.</returns>
        public static Guid? GetData(string name)
        {
            return _state.TryGetValue(name + Thread.CurrentThread.ManagedThreadId, out var data) ? data : null;
        }

        public static bool RemoveData(string name)
        {
            return _state.TryRemove(name+ Thread.CurrentThread.ManagedThreadId, out _);
        }
    }
}
