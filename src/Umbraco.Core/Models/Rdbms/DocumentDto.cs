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

        // fixme writerUserId
        [Column("documentUser")]
        public int WriterUserId { get; set; }

        [Column("versionId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid VersionId { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("releaseDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? ReleaseDate { get; set; }

        [Column("expireDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? ExpiresDate { get; set; }

        [Column("updateDate")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime UpdateDate { get; set; }

        [Column("templateId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(TemplateDto), Column = "nodeId")]
        public int? TemplateId { get; set; }

        // fixme kill that one
        [Column("newest")]
        [Constraint(Default = "0")]
        [Index(IndexTypes.NonClustered, Name = "IX_cmsDocument_newest")]
        public bool Newest { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
        public ContentVersionDto ContentVersionDto { get; set; }

        // fixme wtf
        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
        public DocumentPublishedReadOnlyDto DocumentPublishedReadOnlyDto { get; set; }
    }
}
