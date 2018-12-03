using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.Tag)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class TagDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("group")]
        [Length(100)]
        public string Group { get; set; }

        [Column("tag")]
        [Length(200)]
        [Index(IndexTypes.UniqueNonClustered, ForColumns = "group,tag", Name = "IX_cmsTags")]
        public string Text { get; set; }

        //[Column("key")]
        //[Length(301)] // de-normalized "{group}/{tag}"
        //public string Key { get; set; }

        // queries result column
        [ResultColumn("NodeCount")]
        public int NodeCount { get; set; }
    }
}
