using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(AccessDtoConfiguration))]
public class AccessDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Access;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the access record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the node identifier associated with this access entry.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the node associated with login access.
    /// </summary>
    public int LoginNodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the node to which access is denied.
    /// </summary>
    public int NoAccessNodeId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the access record was created.
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the access record was last updated.
    /// </summary>
    public DateTime UpdateDate { get; set; }

    /// <summary>
    /// Gets or sets the access rules associated with this access entry.
    /// </summary>
    public List<AccessRuleDto> Rules { get; set; } = new();
}
