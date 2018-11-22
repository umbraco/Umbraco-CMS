using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.User2UserGroup)]
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

    [TableName(Constants.DatabaseSchema.Tables.User2UserGroup)]
    [ExplicitColumns]
    internal class User2UserGroupReadOnlyDto
    {
        [Column("userId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_user2userGroup", OnColumns = "userId, userGroupId")]
        [ForeignKey(typeof(UserDto))]
        public int UserId { get; set; }

        [Column("userGroupId")]
        [ForeignKey(typeof(UserGroupDto))]
        public int UserGroupId { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.Foreign)] // fixme
        public UserGroupDto UserGroupDto { get; set; }
    }
}
