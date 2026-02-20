using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey([UserGroupIdColumnName, AppAliasColumnName], AutoIncrement = false)]
[ExplicitColumns]
public class UserGroup2AppDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2App;

    [Obsolete("Use UserGroupIdColumnName instead. Scheduled for removal in Umbraco 18.")]
    public const string PrimaryKeyColumnName = UserGroupIdColumnName;

    public const string UserGroupIdColumnName = "userGroupId";
    private const string AppAliasColumnName = "app";

    [Column(UserGroupIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_userGroup2App", OnColumns = $"{UserGroupIdColumnName}, {AppAliasColumnName}")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }

    [Column(AppAliasColumnName)]
    [Length(50)]
    public string AppAlias { get; set; } = null!;
}
