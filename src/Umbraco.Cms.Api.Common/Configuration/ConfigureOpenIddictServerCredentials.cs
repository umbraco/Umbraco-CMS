using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using Umbraco.Cms.Api.Common.Security;

namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
///     Configures the OpenIddict server with the credentials from the registered <see cref="IOpenIddictServerCredentialsProvider"/>.
/// </summary>
internal sealed class ConfigureOpenIddictServerCredentials : IConfigureOptions<OpenIddictServerOptions>
{
    private readonly IOpenIddictServerCredentialsProvider _credentialsProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureOpenIddictServerCredentials"/> class.
    /// </summary>
    /// <param name="credentialsProvider">The provider of the server credentials.</param>
    public ConfigureOpenIddictServerCredentials(IOpenIddictServerCredentialsProvider credentialsProvider)
        => _credentialsProvider = credentialsProvider;

    /// <inheritdoc/>
    public void Configure(OpenIddictServerOptions options)
    {
        foreach (SigningCredentials credentials in _credentialsProvider.GetSigningCredentials())
        {
            options.SigningCredentials.Add(credentials);
        }

        foreach (EncryptingCredentials credentials in _credentialsProvider.GetEncryptionCredentials())
        {
            options.EncryptionCredentials.Add(credentials);
        }
    }
}
