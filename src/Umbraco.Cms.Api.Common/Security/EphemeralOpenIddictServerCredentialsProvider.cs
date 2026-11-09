using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Umbraco.Cms.Api.Common.Security;

/// <summary>
///     Provides OpenIddict server credentials that are generated per application instance.
/// </summary>
/// <remarks>
///     The credentials are not shared between servers, and change when the application restarts.
/// </remarks>
internal sealed class EphemeralOpenIddictServerCredentialsProvider : IOpenIddictServerCredentialsProvider
{
    private const int SigningKeySizeInBits = 2048;
    private const int EncryptionKeySizeInBytes = 32;

    private readonly SigningCredentials _signingCredentials =
        new(new RsaSecurityKey(RSA.Create(SigningKeySizeInBits)), SecurityAlgorithms.RsaSha256);

    private readonly EncryptingCredentials _encryptionCredentials =
        new(
            new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(EncryptionKeySizeInBytes)),
            SecurityAlgorithms.Aes256KW,
            SecurityAlgorithms.Aes256CbcHmacSha512);

    /// <inheritdoc/>
    public IEnumerable<SigningCredentials> GetSigningCredentials() => [_signingCredentials];

    /// <inheritdoc/>
    public IEnumerable<EncryptingCredentials> GetEncryptionCredentials() => [_encryptionCredentials];
}
