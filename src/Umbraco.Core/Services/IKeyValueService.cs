namespace Umbraco.Core.Services
{
    /// <summary>
    /// Manages the simplified key/value store.
    /// </summary>
    public interface IKeyValueService
    {
        /// <summary>
        /// Gets a value.
        /// </summary>
        /// <remarks>Returns <c>null</c> if no value was found for the key.</remarks>
        string GetValue(string key);

        /// <summary>
        /// Sets a value.
        /// </summary>
        void SetValue(string key, string value);

        /// <summary>
        /// Sets a value.
        /// </summary>
        /// <remarks>Sets the value to <paramref name="newValue"/> if the value is <paramref name="originValue"/>,
        /// and returns true; otherwise throws an exception. In other words, ensures that the value has not changed
        /// before setting it.</remarks>
        void SetValue(string key, string originValue, string newValue);

        /// <summary>
        /// Tries to set a value.
        /// </summary>
        /// <remarks>Sets the value to <paramref name="newValue"/> if the value is <paramref name="originValue"/>,
        /// and returns true; otherwise returns false. In other words, ensures that the value has not changed
        /// before setting it.</remarks>
        bool TrySetValue(string key, string originValue, string newValue);
    }
}
