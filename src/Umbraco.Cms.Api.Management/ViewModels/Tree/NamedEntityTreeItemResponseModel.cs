namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a response model for a tree item that includes a named entity.
/// </summary>
public class NamedEntityTreeItemResponseModel : EntityTreeItemResponseModel
{
    /// <summary>
    /// Gets or sets the name of the entity.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
