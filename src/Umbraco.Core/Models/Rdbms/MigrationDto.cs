using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoMigration")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class MigrationDto
    {
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 100)]
        public int Id { get; set; }

        [Column("name")]
        [Length(255)]
        [Index(IndexTypes.UniqueNonClustered, ForColumns = "name,version", Name = "IX_umbracoMigration")]
        public string Name { get; set; }

        [Column("createDate")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime CreateDate { get; set; }

        [Column("version")]
        [Length(50)]
        public string Version { get; set; }
    }
}