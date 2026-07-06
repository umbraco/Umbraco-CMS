using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Search.Core.Models.ViewModels;

public class DocumentViewModel
{
    public required Guid Id { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required UmbracoObjectTypes ObjectType { get; set; }

    public string? Name { get; set; }

    public string? Icon { get; set; }
}
