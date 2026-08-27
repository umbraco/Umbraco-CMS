using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(UserGroupDtoConfiguration))]
public class UserGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string KeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameKey;

    /// <summary>
    /// Gets or sets the unique identifier for the user group.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier key for the user group.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    /// Gets or sets the alias of the user group.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// Gets or sets the name of the user group.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a textual description providing additional information about the user group.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the default permissions assigned to the user group.
    /// </summary>
    public string? DefaultPermissions { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user group was created.
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user group was last updated.
    /// </summary>
    public DateTime UpdateDate { get; set; }

    /// <summary>
    /// Gets or sets the icon associated with the user group.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user group has access to all languages.
    /// </summary>
    public bool HasAccessToAllLanguages { get; set; }

    /// <summary>
    /// Gets or sets the ID of the root content node that members of the user group start at in the content tree.
    /// A null value indicates no specific start node is set.
    /// </summary>
    public int? StartContentId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the media item that defines the starting point for media access for the user group.
    /// </summary>
    public int? StartMediaId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the element node that defines the starting point for element access for the user group.
    /// </summary>
    public int? StartElementId { get; set; }
}
