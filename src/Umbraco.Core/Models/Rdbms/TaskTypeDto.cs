using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsTaskType")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class TaskTypeDto
    {
        [Column("id")]
        public byte Id { get; set; }

        [Column("alias")]
        public string Alias { get; set; }
    }
}