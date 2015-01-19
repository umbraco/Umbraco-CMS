using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsTask")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class TaskDto
    {
        [Column("closed")]
        [Constraint(Default = "0")]
        public bool Closed { get; set; }

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("taskTypeId")]
        [ForeignKey(typeof(TaskTypeDto))]
        public byte TaskTypeId { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        [Column("parentUserId")]
        [ForeignKey(typeof(UserDto), Name = "FK_cmsTask_umbracoUser")]
        public int ParentUserId { get; set; }

        [Column("userId")]
        [ForeignKey(typeof(UserDto), Name = "FK_cmsTask_umbracoUser1")]
        public int UserId { get; set; }

        [Column("DateTime")]
        [Constraint(Default = "getdate()")]
        public DateTime DateTime { get; set; }

        [Column("Comment")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(500)]
        public string Comment { get; set; }

        [ResultColumn]
        public TaskTypeDto TaskTypeDto { get; set; }
    }
}