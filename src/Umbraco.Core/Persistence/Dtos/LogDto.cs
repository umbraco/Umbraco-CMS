using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.Log)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class LogDto
    {
        private int? _userId;

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("userId")]
        [ForeignKey(typeof(UserDto))]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? UserId { get => _userId == 0 ? null : _userId; set => _userId = value; } //return null if zero

        [Column("NodeId")]
        [Index(IndexTypes.NonClustered, Name = "IX_umbracoLog")]
        public int NodeId { get; set; }

        [Column("Datestamp")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
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
