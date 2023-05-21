namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification for when server variables are parsing
/// </summary>
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
