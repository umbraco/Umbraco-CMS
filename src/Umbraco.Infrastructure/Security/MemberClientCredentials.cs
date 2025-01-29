namespace Umbraco.Cms.Core.Security;

public class MemberClientCredentials
{
    /// <summary>
    ///     Gets or sets the user name of the member to associate with the session after a successful login.
    /// </summary>
    /// <value>The user name of the member.</value>
    public required string UserName { get; set; }

    /// <summary>
    ///     Gets or sets the client ID that allows for a successful login.
    /// </summary>
    /// <value>The client ID.</value>
    public required string ClientId { get; set; }

    /// <summary>
    ///     Gets or sets the client secret that allows for a successful login.
    /// </summary>
    /// <value>The client secret.</value>
    public required string ClientSecret { get; set; }
}
