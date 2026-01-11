using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class UserGroup2AppDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2App;
    public const string PrimaryKeyColumnName = "userGroupId";

    private const string AppAliasName = "app";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_userGroup2App", OnColumns = $"{PrimaryKeyColumnName}, {AppAliasName}")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }

    [Column(AppAliasName)]
    [Length(50)]
    public string AppAlias { get; set; } = null!;
}
