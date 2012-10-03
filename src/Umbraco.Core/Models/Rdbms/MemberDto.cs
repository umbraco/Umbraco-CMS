using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMember")]
    [PrimaryKey("nodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class MemberDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("Email")]
        public string Email { get; set; }

        [Column("LoginName")]
        public string LoginName { get; set; }

        [Column("Password")]
        public string Password { get; set; }
    }
}