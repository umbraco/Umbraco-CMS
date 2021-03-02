using System.Collections.Concurrent;
using System.Threading;

namespace Umbraco.Cms.Core.Scoping
{
    /// <summary>
    /// Represents ambient data that is local to a given asynchronous control flow, such as an asynchronous method.
    /// </summary>
    /// <remarks>
    /// This is just a simple wrapper around <seealso cref="AsyncLocal{T}"/>
    /// </remarks>
    public static class CallContext<T>
    {
        private static readonly ConcurrentDictionary<string, AsyncLocal<T>> s_state = new ConcurrentDictionary<string, AsyncLocal<T>>();

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param>
        public static void SetData(string name, T data) => s_state.GetOrAdd(name, _ => new AsyncLocal<T>()).Value = data;

        //Replace the SetData with the following when you need to debug AsyncLocal. The args.ThreadContextChanged can be usefull
        //public static void SetData(string name, T data) => _state.GetOrAdd(name, _ => new AsyncLocal<T>(OnValueChanged)).Value = data;
        // public static void OnValueChanged(AsyncLocalValueChangedArgs<T> args)
        // {
        //     var typeName = typeof(T).ToString();
        //     Console.WriteLine($"OnValueChanged!, Type: {typeName} Prev: #{args.PreviousValue} Current: #{args.CurrentValue}");
        // }

        /// <summary>
        /// Retrieves an object with the specified name from the <see cref="CallContext{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data being retrieved. Must match the type used when the <paramref name="name"/> was set via <see cref="SetData{T}(string, T)"/>.</typeparam>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or a default value for <typeparamref name="T"/> if none is found.</returns>
        public static T GetData(string name) => s_state.TryGetValue(name, out AsyncLocal<T> data) ? data.Value : default;

        // NOTE: If you have used the old CallContext in the past you might be thinking you need to clean this up but that is not the case.
        // With CallContext you had to call FreeNamedDataSlot to prevent leaks but with AsyncLocal this is not the case, there is no way to clean this up.
        // The above dictionary is sort of a trick because sure, there is always going to be a string key that will exist in the collection but the values
        // themselves are managed per ExecutionContext so they don't build up.
        // There's an SO article relating to this here https://stackoverflow.com/questions/36511243/safety-of-asynclocal-in-asp-net-core

    }
}
