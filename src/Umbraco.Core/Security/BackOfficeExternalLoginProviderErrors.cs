namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Represents errors from an external login provider for the back office.
/// </summary>
public class BackOfficeExternalLoginProviderErrors
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeExternalLoginProviderErrors" /> class.
    /// </summary>
    /// <remarks>Required for deserialization.</remarks>
    public BackOfficeExternalLoginProviderErrors()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeExternalLoginProviderErrors" /> class.
    /// </summary>
    /// <param name="authenticationType">The authentication type of the external provider.</param>
    /// <param name="errors">The collection of error messages.</param>
    public BackOfficeExternalLoginProviderErrors(string? authenticationType, IEnumerable<string> errors)
    {
        AuthenticationType = authenticationType;
        Errors = errors ?? Enumerable.Empty<string>();
    }

    /// <summary>
    ///     Gets or sets the authentication type of the external login provider.
    /// </summary>
    public string? AuthenticationType { get; set; }

    /// <summary>
    ///     Gets or sets the collection of error messages from the external login provider.
    /// </summary>
    public IEnumerable<string>? Errors { get; set; }
}
