using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoRedirectUrl")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    class RedirectUrlDto
    {
        public RedirectUrlDto()
        {
            CreateDateUtc = DateTime.UtcNow;
        }

        // notes
        //
        // we want a unique, non-clustered  index on (url ASC, contentId ASC, createDate DESC) but the
        // problem is that the index key must be 900 bytes max. should we run without an index? done
        // some perfs comparisons, and running with an index on a hash is only slightly slower on
        // inserts, and much faster on reads, so... we have an index on a hash.

        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 1, Name = "PK_umbracoRedirectUrl")]
        public int Id { get; set; }

        [ResultColumn]
        public int ContentId { get; set; }

        [Column("contentKey")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey(typeof(NodeDto), Column = "uniqueID")]
        public Guid ContentKey { get; set; }

        [Column("createDateUtc")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime CreateDateUtc { get; set; }

        [Column("url")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        //[Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRedirectUrl", ForColumns = "url, createDateUtc")]
        public string Url { get; set; }

        [Column("hurl")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRedirectUrl", ForColumns = "hurl, contentKey, createDateUtc")]
        public string Hurl { get; set; }
    }
}
