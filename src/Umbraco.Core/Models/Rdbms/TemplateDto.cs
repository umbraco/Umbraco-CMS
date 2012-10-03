using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsTemplate")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class TemplateDto
    {
        [Column("pk")]
        public int PrimaryKey { get; set; }

        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("master")]
        public int? Master { get; set; }

        [Column("alias")]
        public string Alias { get; set; }

        [Column("design")]
        public string Design { get; set; }
    }
}