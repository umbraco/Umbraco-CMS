using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoLog")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class LogDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("userId")]
        public int UserId { get; set; }

        [Column("NodeId")]
        [Index(IndexTypes.NonClustered, Name = "IX_umbracoLog")]
        public int NodeId { get; set; }

        [Column("Datestamp")]
        [Constraint(Default = "getdate()")]
        public DateTime Datestamp { get; set; }

        [Column("logHeader")]
        [Length(50)]
        public string Header { get; set; }

        [Column("logComment")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(4000)]
        public string Comment { get; set; }
    }
}