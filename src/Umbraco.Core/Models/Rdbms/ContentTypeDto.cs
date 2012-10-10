using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsContentType")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class ContentTypeDto
    {
        [Column("pk")]
        public int PrimaryKey { get; set; }

        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("alias")]
        public string Alias { get; set; }

        [Column("icon")]
        public string Icon { get; set; }

        [Column("thumbnail")]
        public string Thumbnail { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("isContainer")]
        public bool IsContainer { get; set; }

        [Column("allowAtRoot")]
        public bool AllowAtRoot { get; set; }

        [ResultColumn]
        public NodeDto NodeDto { get; set; }
    }
}