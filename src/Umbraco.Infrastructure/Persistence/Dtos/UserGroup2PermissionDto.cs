using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.UserGroup2Permission)]
[ExplicitColumns]
public class UserGroup2PermissionDto
{
    [Column("userGroupId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_userGroup2Permission", OnColumns = "userGroupId, permission")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }

    [Column("permission")]
    [Length(500)]
    public required string Permission { get; set; }
}
