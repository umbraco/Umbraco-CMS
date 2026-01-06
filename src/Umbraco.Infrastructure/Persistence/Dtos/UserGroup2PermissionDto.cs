using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = true)]
[ExplicitColumns]
public class UserGroup2PermissionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2Permission;
    public const string PrimaryKeyName = Constants.DatabaseSchema.PrimaryKeyNameId;

    [Column(PrimaryKeyName)]
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
