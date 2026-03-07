using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

    /// <summary>
    /// Represents the base response model for an item in the recycle bin.
    /// </summary>
public abstract class RecycleBinItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the recycle bin item.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the recycle bin item.
    /// </summary>
    public DateTimeOffset CreateDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the recycle bin item has child items.
    /// </summary>
    [Required]
    public bool HasChildren { get; set; }

    /// <summary>
    /// Gets or sets a reference to the parent item of this recycle bin entry.
    /// This may be <c>null</c> if the item is at the root level of the recycle bin.
    /// </summary>
    public ItemReferenceByIdResponseModel? Parent { get; set; }
}
