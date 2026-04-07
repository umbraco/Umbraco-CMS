using System.Text.Json.Serialization;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Represents the response model containing server status information for the Umbraco Management API.
/// </summary>
public class ServerStatusResponseModel
{
    /// <summary>
    /// Gets or sets the current runtime level of the server, indicating its operational status.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RuntimeLevel ServerStatus { get; set; }
}
