using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoMigration")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class MigrationDto
    {
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("name")]
        [Length(255)]
        [Index(IndexTypes.UniqueNonClustered, ForColumns = "name,version", Name = "IX_umbracoMigration")]
        public string Name { get; set; }

        [Column("createDate")]
        [Constraint(Default = "getdate()")]
        public DateTime CreateDate { get; set; }

        [Column("version")]
        [Length(50)]
        public string Version { get; set; }
    }
}