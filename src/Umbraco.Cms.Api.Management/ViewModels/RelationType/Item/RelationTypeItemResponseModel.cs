using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.RelationType.Item;

    /// <summary>
    /// Represents the response model returned by the API for a relation type item in Umbraco.
    /// </summary>
public class RelationTypeItemResponseModel : NamedItemResponseModelBase
{
    /// <summary>
    /// Gets or sets a value indicating whether this relation type can be deleted.
    /// </summary>
    public bool IsDeletable { get; set; }
}
