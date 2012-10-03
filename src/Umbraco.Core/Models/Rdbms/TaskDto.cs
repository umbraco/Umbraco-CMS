using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsTask")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class TaskDto
    {
        [Column("closed")]
        public bool Closed { get; set; }

        [Column("id")]
        public int Id { get; set; }

        [Column("taskTypeId")]
        public byte TaskTypeId { get; set; }

        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("parentUserId")]
        public int ParentUserId { get; set; }

        [Column("userId")]
        public int UserId { get; set; }

        [Column("DateTime")]
        public DateTime DateTime { get; set; }

        [Column("Comment")]
        public string Comment { get; set; }
    }
}