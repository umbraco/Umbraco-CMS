using Umbraco.Cms.Core;

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
    ///     Generates a new cryptographic HMAC secret key and persists it to the configuration file
    ///     if one is not already configured. No-ops when a key already exists to avoid rotating
    ///     the key and invalidating previously generated signed URLs.
    /// </summary>
    /// <returns>
    ///     An <see cref="Attempt{TResult}" /> with status <see cref="HmacSecretKeyOperationStatus.Success" /> if the key was
    ///     generated and persisted, <see cref="HmacSecretKeyOperationStatus.KeyExists" /> if a key already exists, or
    ///     <see cref="HmacSecretKeyOperationStatus.Error" /> if persistence failed.
    /// </returns>
    Task<Attempt<HmacSecretKeyOperationStatus>> CreateHmacSecretKeyAsync();
}
