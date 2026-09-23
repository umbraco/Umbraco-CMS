namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents the API request model used to create an Element Library element from a block held in a
/// content item's property, reproducing the cultures the block is currently live in.
/// </summary>
/// <remarks>
/// The block's values are read from the owner's stored property value, so unsaved editor changes are not
/// carried over.
/// </remarks>
public class CreateElementFromBlockRequestModel
{
    /// <summary>
    /// Gets or sets the id to give the new element, or <c>null</c> to have one assigned.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets a reference to the content item that owns the property the block is nested in.
    /// </summary>
    public required ReferenceByIdModel Owner { get; set; }

    /// <summary>
    /// Gets or sets a reference to the block to create the element from.
    /// </summary>
    public required ReferenceByIdModel Block { get; set; }

    /// <summary>
    /// Gets or sets a reference to the Library folder to create the new element in, or <c>null</c> for the root.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }

    /// <summary>
    /// Gets or sets the name to give the new element.
    /// </summary>
    public required string Name { get; set; }
}
