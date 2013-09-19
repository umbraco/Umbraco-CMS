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
        
        [Column("editorAlias")]        
        public string EditorAlias { get; set; }

        [Column("macro")]
        [ForeignKey(typeof(MacroDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsMacroProperty_Alias", ForColumns = "macro, macroPropertyAlias")]
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