using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsContentVersion")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    public class ContentVersionDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("ContentId")]
        [ForeignKey(typeof(ContentDto), Column = "nodeId")]
        public int NodeId { get; set; }

        [Column("VersionId")]
        [Index(IndexTypes.UniqueNonClustered)]
        public Guid VersionId { get; set; }

        [Column("VersionDate")]
        [Constraint(Default = "getdate()")]
        public DateTime VersionDate { get; set; }

        [Column("LanguageLocale")]
        [Length(10)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Language { get; set; }

        [ResultColumn]
        public ContentDto ContentDto { get; set; }
    }
}