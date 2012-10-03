using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUserType")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class UserTypeDto
    {
        [Column("id")]
        public short Id { get; set; }

        [Column("userTypeAlias")]
        public string Alias { get; set; }

        [Column("userTypeName")]
        public string Name { get; set; }

        [Column("userTypeDefaultPermissions")]
        public string DefaultPermissions { get; set; }
    }
}