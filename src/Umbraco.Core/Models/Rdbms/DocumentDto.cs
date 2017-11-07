using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName(TableName)]
    [PrimaryKey("versionId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class DocumentDto
    {
        private const string TableName = Constants.DatabaseSchema.Tables.Document;

        [Column("nodeId")]
        [ForeignKey(typeof(ContentDto), Column = "nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_NodeId", ForColumns = "nodeId, versionId")]
        public int NodeId { get; set; }

        [Column("published")]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Published")]
        public bool Published { get; set; }

        [Column("writerUserId")]
        public int WriterUserId { get; set; }

        [Column("releaseDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? ReleaseDate { get; set; }

        [Column("expireDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? ExpiresDate { get; set; }

        [Column("updateDate")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime UpdateDate { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
        public ContentDto ContentDto { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")] // FIXME not one-to-one! BUT it depends on the query!
        public DocumentVersionDto DocumentVersionDto { get; set; }
    }
}
