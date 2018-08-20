using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [Obsolete("This is no longer used and will be removed from Umbraco in future versions")]
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
        [Length(50)]
        public string Alias { get; set; }

        [Column("stylesheetPropertyValue")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(400)]
        public string Value { get; set; }
    }
}