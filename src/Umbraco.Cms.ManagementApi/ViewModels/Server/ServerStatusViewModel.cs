using System.Text.Json.Serialization;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.ManagementApi.ViewModels.Server;

public class ServerStatusViewModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RuntimeLevel ServerStatus { get; set; }
}
