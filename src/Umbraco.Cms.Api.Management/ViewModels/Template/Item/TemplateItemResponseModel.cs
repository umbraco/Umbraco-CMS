using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Template.Item;

/// <summary>
/// Represents a response model containing details about a template item returned by the management API.
/// Used to transfer template item data between the server and client.
/// </summary>
public class TemplateItemResponseModel : NamedItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the alias of the template.
    /// </summary>
    public required string Alias { get; set; }
}
