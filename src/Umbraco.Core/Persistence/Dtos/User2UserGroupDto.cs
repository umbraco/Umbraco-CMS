using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [ExplicitColumns]
    internal class User2UserGroupDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.User2UserGroup;

        [Column("userId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_user2userGroup", OnColumns = "userId, userGroupId")]
        [ForeignKey(typeof(UserDto))]
        public int UserId { get; set; }

        [Column("userGroupId")]
        [ForeignKey(typeof(UserGroupDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_UserGroupId")]
        public int UserGroupId { get; set; }
    }
}
