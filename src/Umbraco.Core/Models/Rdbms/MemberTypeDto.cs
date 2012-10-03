using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMemberType")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class MemberTypeDto
    {
        [Column("pk")]
        public int PrimaryKey { get; set; }

        [Column("NodeId")]
        public int NodeId { get; set; }

        [Column("propertytypeId")]
        public int PropertyTypeId { get; set; }

        [Column("memberCanEdit")]
        public bool CanEdit { get; set; }

        [Column("viewOnProfile")]
        public bool ViewOnProfile { get; set; }
    }
}