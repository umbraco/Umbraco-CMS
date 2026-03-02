namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Used to get and create the imaging HMAC secret key.
/// </summary>
public interface IHmacSecretKeyService
{
    /// <summary>
    ///     Checks whether a non-empty HMAC secret key is configured.
    /// </summary>
    bool HasHmacSecretKey();

    /// <summary>
    ///     Generates a new cryptographic HMAC secret key and persists it to the configuration file.
    /// </summary>
    /// <returns><c>true</c> if the key was successfully created and persisted; otherwise <c>false</c>.</returns>
    Task<bool> TryCreateHmacSecretKeyAsync();
}
