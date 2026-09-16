using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Search.Core.Models.ViewModels;

/// <summary>
/// The Management API representation of a single search hit.
/// </summary>
public class DocumentViewModel
{
    /// <summary>
    /// Gets or sets the key of the matched item.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the entity type of the matched item.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required UmbracoObjectTypes ObjectType { get; set; }

    /// <summary>
    /// Gets or sets the name of the matched item, if available.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the icon of the matched item, if available.
    /// </summary>
    public string? Icon { get; set; }
}
