using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(TagRelationshipDtoConfiguration))]
public class TagRelationshipDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.TagRelationship;
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string TagIdColumnName = "tagId";
    public const string PropertyTypeIdColumnName = "propertyTypeId";

    /// <summary>Gets or sets the node identifier associated with the tag relationship.</summary>
    public int NodeId { get; set; }

    /// <summary>Gets or sets the identifier of the tag.</summary>
    public int TagId { get; set; }

    /// <summary>Gets or sets the identifier of the property type associated with this tag relationship.</summary>
    public int PropertyTypeId { get; set; }
}
