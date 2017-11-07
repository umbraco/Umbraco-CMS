using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class ContentVersionDto
    {
        private const string TableName = Constants.DatabaseSchema.Tables.ContentVersion;

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(ContentDto), Column = "nodeId")]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_NodeId")]
        public int NodeId { get; set; }

        [Column("versionId")]
        [Index(IndexTypes.UniqueNonClustered)]
        public Guid VersionId { get; set; }

        [Column("versionDate")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime VersionDate { get; set; }

        [Column("current")]
        [Constraint(Default = false)] // fixme or true? unique index on NodeId,current!!
        public bool Current { get; set; }

        [Column("text")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Text { get; set; }

        // fixme assess whether we really want this
        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "NodeId", ReferenceMemberName = "NodeId")]
        public ContentDto ContentDto { get; set; }
    }
}
