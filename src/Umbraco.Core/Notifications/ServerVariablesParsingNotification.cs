namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification for when server variables are parsing
/// </summary>
[Obsolete("No longer published, as the AngularJS backoffice and its server variables were removed, so handlers of this notification never run. Serve values the backoffice needs from a Management API endpoint instead. Scheduled for removal in Umbraco 20.")]
public class ServerVariablesParsingNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerVariablesParsingNotification" /> class.
    /// </summary>
    public ServerVariablesParsingNotification(IDictionary<string, object> serverVariables) =>
        ServerVariables = serverVariables;

    /// <summary>
    ///     Gets a mutable dictionary of server variables
    /// </summary>
    public IDictionary<string, object> ServerVariables { get; }
}
