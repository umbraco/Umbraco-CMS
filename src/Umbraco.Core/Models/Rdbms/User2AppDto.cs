using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUser2app")]
    [PrimaryKey("user", autoIncrement = false)]
    [ExplicitColumns]
    internal class User2AppDto
    {
        [Column("user")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_user2app", OnColumns = "[user], [app]")]
        [ForeignKey(typeof(UserDto))]
        public int UserId { get; set; }

        [Column("app")]
        [DatabaseType(DatabaseTypes.Nvarchar, Length = 50)]
        public string AppAlias { get; set; }
    }
}