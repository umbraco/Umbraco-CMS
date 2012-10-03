using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUser2app")]
    [PrimaryKey("user", autoIncrement = false)]
    [ExplicitColumns]
    internal class User2AppDto
    {
        [Column("user")]
        public int UserId { get; set; }

        [Column("app")]
        public string AppAlias { get; set; }
    }
}