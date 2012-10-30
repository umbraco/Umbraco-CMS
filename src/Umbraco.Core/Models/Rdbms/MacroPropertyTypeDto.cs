using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMacroPropertyType")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class MacroPropertyTypeDto
    {
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 26)]
        public short Id { get; set; }

        [Column("macroPropertyTypeAlias")]
        [Length(50)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Alias { get; set; }

        [Column("macroPropertyTypeRenderAssembly")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string RenderAssembly { get; set; }

        [Column("macroPropertyTypeRenderType")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string RenderType { get; set; }

        [Column("macroPropertyTypeBaseType")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string BaseType { get; set; }
    }
}