using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsTags")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class TagDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("tag")]
        public string Tag { get; set; }

        [Column("ParentId")]
        public int? ParentId { get; set; }

        [Column("group")]
        public string Group { get; set; }
    }
}