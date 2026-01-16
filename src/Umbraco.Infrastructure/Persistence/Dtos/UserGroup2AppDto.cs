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

    internal const string ReferenceMemberName = "UserGroupId"; // should be PrimaryKeyColumnName, but for database compatibility we keep it like this

    private const string AppAliasColumnName = "app";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_userGroup2App", OnColumns = $"{PrimaryKeyColumnName}, {AppAliasColumnName}")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }

    [Column(AppAliasColumnName)]
    [Length(50)]
    public string AppAlias { get; set; } = null!;
}
