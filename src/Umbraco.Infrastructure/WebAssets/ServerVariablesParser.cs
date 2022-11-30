using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.WebAssets;

/// <summary>
///     Ensures the server variables are included in the outgoing JS script
/// </summary>
public class ServerVariablesParser
{
    private const string Token = "##Variables##";
    private readonly IEventAggregator _eventAggregator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerVariablesParser" /> class.
    /// </summary>
    public ServerVariablesParser(IEventAggregator eventAggregator) => _eventAggregator = eventAggregator;

    /// <summary>
    ///     Ensures the server variables in the dictionary are included in the outgoing JS script
    /// </summary>
    public async Task<string> ParseAsync(Dictionary<string, object> items)
    {
        var vars = Resources.ServerVariables;

        // Raise event for developers to add custom variables
        await _eventAggregator.PublishAsync(new ServerVariablesParsingNotification(items));

        var json = JObject.FromObject(items);
        return vars.Replace(Token, json.ToString());
    }
}
