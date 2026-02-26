using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages the simplified key/value store.
/// </summary>
public interface IKeyValueService
{
    /// <summary>
    ///     Gets a value.
    /// </summary>
    /// <remarks>Returns <c>null</c> if no value was found for the key.</remarks>
    Task<string?> GetValue(string key);

    /// <summary>
    ///     Returns key/value pairs for all keys with the specified prefix.
    /// </summary>
    /// <param name="keyPrefix"></param>
    /// <returns></returns>
    Task<Attempt<IReadOnlyDictionary<string, string?>?, KeyValueOperationStatus>> FindByKeyPrefix(string keyPrefix);

    /// <summary>
    ///     Sets a value.
    /// </summary>
    Task<Attempt<KeyValueOperationStatus>> SetValue(string key, string value);

    /// <summary>
    ///     Sets a value.
    /// </summary>
    /// <remarks>
    ///     Sets the value to <paramref name="newValue" /> if the value is <paramref name="originValue" />,
    ///     and returns true; otherwise throws an exception. In other words, ensures that the value has not changed
    ///     before setting it.
    /// </remarks>
    Task<Attempt<KeyValueOperationStatus>> SetValue(string key, string originValue, string newValue);

    /// <summary>
    ///     Tries to set a value.
    /// </summary>
    /// <remarks>
    ///     Sets the value to <paramref name="newValue" /> if the value is <paramref name="originValue" />,
    ///     and returns true; otherwise returns false. In other words, ensures that the value has not changed
    ///     before setting it.
    /// </remarks>
    Task<Attempt<bool, KeyValueOperationStatus>> TrySetValue(string key, string originValue, string newValue);
}
