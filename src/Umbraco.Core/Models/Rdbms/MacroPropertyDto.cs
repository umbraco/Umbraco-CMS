using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMacroProperty")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class MacroPropertyDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        //NOTE: This column is not used, we always show the properties
        [Column("macroPropertyHidden")]
        [Constraint(Default = "0")]
        public bool Hidden { get; set; }

        [Column("macroPropertyType")]
        [ForeignKey(typeof(MacroPropertyTypeDto))]
        public short Type { get; set; }

        [Column("macro")]
        [ForeignKey(typeof(MacroDto))]
        public int Macro { get; set; }

        [Column("macroPropertySortOrder")]
        [Constraint(Default = "0")]
        public byte SortOrder { get; set; }

        [Column("macroPropertyAlias")]
        [Length(50)]
        public string Alias { get; set; }

        [Column("macroPropertyName")]
        public string Name { get; set; }
    }
}