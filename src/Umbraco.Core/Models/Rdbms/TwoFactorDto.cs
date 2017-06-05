using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoTwoFactorAuthentication")]
    [PrimaryKey("userId", autoIncrement = false)]
    public class TwoFactorDto
    {
        [Column("userId")]
        public int UserId { get; set; }

        [Column("key")]
        public string Key { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("confirmed")]
        public bool Confirmed { get; set; }
    }
}
