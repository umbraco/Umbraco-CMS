using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.UserGroup2App)]
    [ExplicitColumns]
    internal class UserGroup2AppDto
    {
        [Column("userGroupId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_userGroup2App", OnColumns = "userGroupId, app")]
        [ForeignKey(typeof(UserGroupDto))]
        public int UserGroupId { get; set; }

        [Column("app")]
        [Length(50)]
        public string AppAlias { get; set; }
    }
}
