using Microsoft.IdentityModel.Tokens;

namespace Umbraco.Cms.Api.Common.Security;

/// <summary>
///     Provides the signing and encryption credentials used by the OpenIddict server.
/// </summary>
/// <remarks>
///     <para>
///         Tokens issued in the ASP.NET Core Data Protection format do not use these credentials, but identity tokens
///         are always issued as JWTs signed with the signing credentials, and the public signing keys are published
///         from the <c>/.well-known/jwks</c> endpoint.
///     </para>
///     <para>
///         The default implementation generates ephemeral credentials per application instance. In a load balanced
///         setup where external clients validate identity tokens, replace it with an implementation that returns the
///         same credentials on every server, for example from an X.509 certificate.
///     </para>
///     <para>
///         The credentials are read when the OpenIddict server options are built. Multiple credentials can be
///         returned to support key rollover.
///     </para>
///     <para>
///         The provider is resolved from the root service provider, so implementations must not depend on scoped
///         services. Register a replacement with <c>AddUnique</c> from a composer. Credentials registered directly on
///         the OpenIddict server options (for example with <c>AddSigningCertificate</c>) are used alongside those from
///         the provider, rather than in place of them, so provide them from here instead.
///     </para>
/// </remarks>
public interface IOpenIddictServerCredentialsProvider
{
    /// <summary>
    ///     Gets the credentials used to sign tokens. At least one asymmetric signing credential is required.
    /// </summary>
    /// <returns>The signing credentials.</returns>
    IEnumerable<SigningCredentials> GetSigningCredentials();

    /// <summary>
    ///     Gets the credentials used to encrypt tokens. At least one encryption credential is required.
    /// </summary>
    /// <returns>The encryption credentials.</returns>
    IEnumerable<EncryptingCredentials> GetEncryptionCredentials();
}
