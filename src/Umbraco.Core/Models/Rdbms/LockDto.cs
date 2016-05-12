using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoLock")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class LockDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("value")]
        public int Value { get; set; } = 1;

        [Column("name)")]
        public string Name { get; set; }
    }
}