using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.TaskType)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class TaskTypeDto
    {
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 2)]
        public byte Id { get; set; }

        [Column("alias")]
        [Index(IndexTypes.NonClustered)]
        public string Alias { get; set; }
    }
}
