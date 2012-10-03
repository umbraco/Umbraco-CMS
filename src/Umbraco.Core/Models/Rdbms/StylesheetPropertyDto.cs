using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsStylesheetProperty")]
    [PrimaryKey("nodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class StylesheetPropertyDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("stylesheetPropertyEditor")]
        public bool? Editor { get; set; }

        [Column("stylesheetPropertyAlias")]
        public string Alias { get; set; }

        [Column("stylesheetPropertyValue")]
        public string Value { get; set; }
    }
}