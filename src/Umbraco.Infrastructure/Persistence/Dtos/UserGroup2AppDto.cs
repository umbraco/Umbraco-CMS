using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = false)]
[ExplicitColumns]
public class UserGroup2AppDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2App;
    public const string PrimaryKeyName = "userGroupId"; // Constants.DatabaseSchema.PrimaryKeyNameId;

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_userGroup2App", OnColumns = "userGroupId, app")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }

    [Column("app")]
    [Length(50)]
    public string AppAlias { get; set; } = null!;
}
