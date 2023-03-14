using System.Text.Json.Serialization;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Server;

public class ServerStatusResponseModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RuntimeLevel ServerStatus { get; set; }
}
