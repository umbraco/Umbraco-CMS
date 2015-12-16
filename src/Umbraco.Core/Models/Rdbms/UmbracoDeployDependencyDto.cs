using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoDeployDependency")]
    [ExplicitColumns]
    internal class UmbracoDeployDependencyDto
    {   
        [Column("sourceId")]
        [PrimaryKeyColumn(AutoIncrement = false, Clustered = true, Name = "PK_umbracoDeployDependency", OnColumns = "sourceId, targetId")]
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