namespace Umbraco.Cms.Core.Models.ServerEvents;

/// <summary>
///     Represents the result of server event authorization, indicating which event sources the user can and cannot access.
/// </summary>
public class SeverEventAuthorizationResult
{
    /// <summary>
    /// The list of events the user should be authorized to listen to
    /// </summary>
    public required IEnumerable<string> AuthorizedEventSources { get; set; }

    /// <summary>
    /// The list of events the user should not be authorized to listen to
    /// </summary>
    public required IEnumerable<string> UnauthorizedEventSources { get; set; }
}
