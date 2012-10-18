using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsStylesheetProperty")]
    [PrimaryKey("nodeId", autoIncrement = false)]
    [ExplicitColumns]
    internal class StylesheetPropertyDto
    {
        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public int NodeId { get; set; }

        [Column("stylesheetPropertyEditor")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public bool? Editor { get; set; }

        [Column("stylesheetPropertyAlias")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [DatabaseType(DatabaseTypes.Nvarchar, Length = 50)]
        public string Alias { get; set; }

        [Column("stylesheetPropertyValue")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [DatabaseType(DatabaseTypes.Nvarchar, Length = 400)]
        public string Value { get; set; }
    }
}