using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
public class UserGroup2PermissionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2Permission;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string PermissionColumnName = "permission";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_userGroup2Permission", AutoIncrement = true)]
    public int Id { get; set; }

    [Column("userGroupKey")]
    [Index(IndexTypes.NonClustered, IncludeColumns = PermissionColumnName)]
    [ForeignKey(typeof(UserGroupDto), Column = UserGroupDto.KeyColumnName)]
    public Guid UserGroupKey { get; set; }

    [Column(PermissionColumnName)]
    [Length(255)]
    public required string Permission { get; set; }
}
