namespace Umbraco.Cms.Core.Security;

/// <inheritdoc />
public class ExternalLogin : IExternalLogin
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalLogin" /> class.
    /// </summary>
    public ExternalLogin(string loginProvider, string providerKey, string? userData = null)
    {
        LoginProvider = loginProvider ?? throw new ArgumentNullException(nameof(loginProvider));
        ProviderKey = providerKey ?? throw new ArgumentNullException(nameof(providerKey));
        UserData = userData;
    }

    /// <inheritdoc />
    public string LoginProvider { get; }

    /// <inheritdoc />
    public string ProviderKey { get; }

    /// <inheritdoc />
    public string? UserData { get; }
}
