using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoLog")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class LogDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("userId")]
        public int UserId { get; set; }

        [Column("NodeId")]
        public int NodeId { get; set; }

        [Column("Datestamp")]
        public DateTime Datestamp { get; set; }

        [Column("logHeader")]
        public string Header { get; set; }

        [Column("logComment")]
        public string Comment { get; set; }
    }
}