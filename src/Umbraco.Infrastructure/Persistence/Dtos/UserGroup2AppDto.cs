using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = false)]
[ExplicitColumns]
public class UserGroup2AppDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2App;

    [Column("userGroupId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_userGroup2App", OnColumns = "userGroupId, app")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }

    [Column("app")]
    [Length(50)]
    public string AppAlias { get; set; } = null!;
}
