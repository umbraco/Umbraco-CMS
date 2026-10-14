using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(ContentDtoConfiguration))]
public class ContentDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Content;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string ContentTypeIdColumnName = "contentTypeId";

    /// <summary>
    /// Gets or sets the identifier of the node.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the content type.
    /// </summary>
    public int ContentTypeId { get; set; }
}
