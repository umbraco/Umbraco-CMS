using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsTemplate")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class TemplateDto
    {
        [Column("pk")]
        [PrimaryKeyColumn]
        public int PrimaryKey { get; set; }

        [Column("nodeId")]
        [Index(IndexTypes.UniqueNonClustered)]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        [Column("master")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(TemplateDto))]
        public int? Master { get; set; }

        [Column("alias")]
        [DatabaseType(DatabaseTypes.NVARCHAR, Length = 100)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Alias { get; set; }

        [Column("design")]
        [DatabaseType(DatabaseTypes.NTEXT)]
        public string Design { get; set; }
    }
}