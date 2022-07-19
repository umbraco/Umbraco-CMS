namespace Umbraco.Cms.Core.Security;

/// <inheritdoc />
public class ExternalLoginToken : IExternalLoginToken
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalLoginToken" /> class.
    /// </summary>
    public ExternalLoginToken(string loginProvider, string name, string value)
    {
        LoginProvider = loginProvider ?? throw new ArgumentNullException(nameof(loginProvider));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc />
    public string LoginProvider { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Value { get; }
}
