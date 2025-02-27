using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.UserGroup2Permission)]
[ExplicitColumns]
public class UserGroup2PermissionDto
{
    [Column("id")]
    [PrimaryKeyColumn(Name = "PK_userGroup2Permission", AutoIncrement = true)]
    public int Id { get; set; }

    [Column("userGroupKey")]
    [Index(IndexTypes.NonClustered, IncludeColumns = "permission")]
    [ForeignKey(typeof(UserGroupDto), Column = "key")]
    public Guid UserGroupKey { get; set; }

    [Column("permission")]
    [Length(255)]
    public required string Permission { get; set; }
}
