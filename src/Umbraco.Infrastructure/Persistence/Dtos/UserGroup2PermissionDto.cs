using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.UserGroup2Permission)]
[ExplicitColumns]
public class UserGroup2PermissionDto
{
    [PrimaryKeyColumn(Name = "PK_userGroup2Permission", AutoIncrement = true)]
    public int Id { get; set; }

    [Column("userGroupId")]
    [Index(IndexTypes.NonClustered, IncludeColumns = "permission")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }

    [Column("permission")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public required string Permission { get; set; }
}
