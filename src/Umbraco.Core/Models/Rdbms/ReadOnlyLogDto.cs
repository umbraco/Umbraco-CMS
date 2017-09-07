using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Models.Rdbms
{
    /// <summary>
    /// object used for returning data from the umbracoLog table
    /// </summary>
    [TableName("umbracoLog")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class ReadOnlyLogDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
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

        [ResultColumn("userName")]
        public string UserName { get; set; }

        [ResultColumn("userAvatar")]
        public string UserAvatar { get; set; }
    }
}