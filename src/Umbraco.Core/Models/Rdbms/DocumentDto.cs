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
        [ForeignKey(typeof(ContentDto))]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsDocument", ForColumns = "[nodeId], [versionId]")]
        public int NodeId { get; set; }

        [Column("published")]
        public bool Published { get; set; }

        [Column("documentUser")]
        public int UserId { get; set; }

        [Column("versionId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid VersionId { get; set; }

        [Column("text")]
        [DatabaseType(DatabaseTypes.Nvarchar, Length = 255)]
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
        [ForeignKey(typeof(TemplateDto))]
        public int? TemplateId { get; set; }

        [Column("alias")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [DatabaseType(DatabaseTypes.Nvarchar, Length = 255)]
        public string Alias { get; set; }

        [Column("newest")]
        [Constraint(Default = "0")]
        public bool Newest { get; set; }

        [ResultColumn]
        public ContentVersionDto ContentVersionDto { get; set; }
    }
}