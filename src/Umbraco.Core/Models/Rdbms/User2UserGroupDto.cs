using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUser2UserGroup")]
    [ExplicitColumns]
    internal class User2UserGroupDto
    {
        [Column("userId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_user2userGroup", OnColumns = "userId, userGroupId")]
        [ForeignKey(typeof(UserDto))]
        public int UserId { get; set; }

        [Column("userGroupId")]
        [ForeignKey(typeof(UserGroupDto))]
        public int UserGroupId { get; set; }
    }
}