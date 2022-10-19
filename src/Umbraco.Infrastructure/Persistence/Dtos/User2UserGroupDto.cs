using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.User2UserGroup)]
[ExplicitColumns]
public class User2UserGroupDto
{
    [Column("userId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_user2userGroup", OnColumns = "userId, userGroupId")]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    [Column("userGroupId")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }
}
