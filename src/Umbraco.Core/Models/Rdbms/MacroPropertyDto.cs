using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMacroProperty")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class MacroPropertyDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("macroPropertyHidden")]
        public bool Hidden { get; set; }

        [Column("macroPropertyType")]
        public short Type { get; set; }

        [Column("macro")]
        public int Macro { get; set; }

        [Column("macroPropertySortOrder")]
        public byte SortOrder { get; set; }

        [Column("macroPropertyAlias")]
        public string Alias { get; set; }

        [Column("macroPropertyName")]
        public string Name { get; set; }
    }
}