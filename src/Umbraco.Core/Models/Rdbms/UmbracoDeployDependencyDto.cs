using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoDeployDependency")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class UmbracoDeployDependencyDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_umbracoDeployDependency")]
        public int Id { get; set; }

        [Column("sourceId")]
        [ForeignKey(typeof(UmbracoDeployChecksumDto), Name = "FK_umbracoDeployDependency_umbracoDeployChecksum_id1")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int SourceId { get; set; }

        [Column("targetId")]
        [ForeignKey(typeof(UmbracoDeployChecksumDto), Name = "FK_umbracoDeployDependency_umbracoDeployChecksum_id2")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int TargetId { get; set; }

        [Column("mode")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int Mode { get; set; }

    }
}