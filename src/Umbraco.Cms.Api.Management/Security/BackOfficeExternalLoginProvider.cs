using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     An external login (OAuth) provider for the back office
/// </summary>
public class BackOfficeExternalLoginProvider : IEquatable<BackOfficeExternalLoginProvider>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeExternalLoginProvider"/> class with the specified authentication type and options.
    /// </summary>
    /// <param name="authenticationType">The authentication type used by the external login provider.</param>
    /// <param name="properties">An <see cref="IOptionsMonitor{TOptions}"/> instance for monitoring <see cref="BackOfficeExternalLoginProviderOptions"/>.</param>
    public BackOfficeExternalLoginProvider(
        string authenticationType,
        IOptionsMonitor<BackOfficeExternalLoginProviderOptions> properties)
    {
        if (properties is null)
        {
            throw new ArgumentNullException(nameof(properties));
        }

        AuthenticationType = authenticationType ?? throw new ArgumentNullException(nameof(authenticationType));
        Options = properties.Get(authenticationType);
    }

    /// <summary>
    ///     The authentication "Scheme"
    /// </summary>
    public string AuthenticationType { get; }

    /// <summary>
    /// Gets the <see cref="BackOfficeExternalLoginProviderOptions"/> that configure this back office external login provider.
    /// </summary>
    public BackOfficeExternalLoginProviderOptions Options { get; }

    /// <summary>
    /// Determines whether the specified <see cref="BackOfficeExternalLoginProvider"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="BackOfficeExternalLoginProvider"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified <paramref name="other"/> is equal to the current instance; otherwise, <c>false</c>.</returns>
    public bool Equals(BackOfficeExternalLoginProvider? other) =>
        other != null && AuthenticationType == other.AuthenticationType;

    /// <summary>
    /// Determines whether the specified object is equal to the current BackOfficeExternalLoginProvider.
    /// </summary>
    /// <param name="obj">The object to compare with the current BackOfficeExternalLoginProvider.</param>
    /// <returns>true if the specified object is equal to the current BackOfficeExternalLoginProvider; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as BackOfficeExternalLoginProvider);
    /// <summary>Returns a hash code for this instance.</summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => HashCode.Combine(AuthenticationType);
}
