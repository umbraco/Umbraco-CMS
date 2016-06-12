using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoContentUrlRule")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    class ContentUrlRuleDto
    {
        public ContentUrlRuleDto()
        {
            CreateDateUtc = DateTime.UtcNow;
        }

        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 1, Name = "PK_umbracoContentUrlRule")]
        public int Id { get; set; }

        [Column("contentId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey(typeof(NodeDto), Column = "id")]
        public int ContentId { get; set; }

        [Column("createDateUtc")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime CreateDateUtc { get; set; }

        [Column("url")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoContentUrlRule", ForColumns = "url, createDateUtc")]
        public string Url { get; set; }
    }
}
