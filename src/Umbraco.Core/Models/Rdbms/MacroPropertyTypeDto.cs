using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMacroPropertyType")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class MacroPropertyTypeDto
    {
        [Column("id")]
        public short Id { get; set; }

        [Column("macroPropertyTypeAlias")]
        public string Alias { get; set; }

        [Column("macroPropertyTypeRenderAssembly")]
        public string RenderAssembly { get; set; }

        [Column("macroPropertyTypeRenderType")]
        public string RenderType { get; set; }

        [Column("macroPropertyTypeBaseType")]
        public string BaseType { get; set; }
    }
}