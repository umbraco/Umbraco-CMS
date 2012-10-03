using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsDocument")]
    [PrimaryKey("versionId", autoIncrement = false)]
    [ExplicitColumns]
    internal class DocumentDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("published")]
        public bool Published { get; set; }

        [Column("documentUser")]
        public int UserId { get; set; }

        [Column("versionId")]
        public Guid VersionId { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("releaseDate")]
        public DateTime? ReleaseDate { get; set; }

        [Column("expireDate")]
        public DateTime? ExpiresDate { get; set; }

        [Column("updateDate")]
        public DateTime UpdateDate { get; set; }

        [Column("templateId")]
        public int? TemplateId { get; set; }

        [Column("alias")]
        public string Alias { get; set; }

        [Column("newest")]
        public bool Newest { get; set; }

        [ResultColumn]
        public ContentVersionDto ContentVersionDto { get; set; }
    }
}