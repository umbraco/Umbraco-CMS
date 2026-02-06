using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class User2UserGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User2UserGroup;
    public const string PrimaryKeyColumnName = "PK_user2userGroup";

    private const string UserIdColumnName = "userId";
    private const string UserGroupIdColumnName = "userGroupId";

    [Column(UserIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = PrimaryKeyColumnName, OnColumns = $"{UserIdColumnName}, {UserGroupIdColumnName}")]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    [Column(UserGroupIdColumnName)]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }
}
