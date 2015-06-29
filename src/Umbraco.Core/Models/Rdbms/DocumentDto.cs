using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsDocument")]
    [PrimaryKey("versionId", autoIncrement = false)]
    [ExplicitColumns]
    internal class DocumentDto
    {
        [Column("nodeId")]
        [ForeignKey(typeof(ContentDto), Column = "nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsDocument", ForColumns = "nodeId, versionId")]
        public int NodeId { get; set; }

        [Column("published")]
        [Index(IndexTypes.NonClustered, Name = "IX_cmsDocument_published")]
        public bool Published { get; set; }

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
        [Constraint(Default = "getdate()")]
        public DateTime UpdateDate { get; set; }

        [Column("templateId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(TemplateDto), Column = "nodeId")]
        public int? TemplateId { get; set; }
        
        [Column("newest")]
        [Constraint(Default = "0")]
        [Index(IndexTypes.NonClustered, Name = "IX_cmsDocument_newest")]
        public bool Newest { get; set; }

        [ResultColumn]
        public ContentVersionDto ContentVersionDto { get; set; }

        [ResultColumn]
        public DocumentPublishedReadOnlyDto DocumentPublishedReadOnlyDto { get; set; }
    }
}