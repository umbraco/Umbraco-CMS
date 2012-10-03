using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMacro")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class MacroDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("macroUseInEditor")]
        public bool UseInEditor { get; set; }

        [Column("macroRefreshRate")]
        public int RefreshRate { get; set; }

        [Column("macroAlias")]
        public string Alias { get; set; }

        [Column("macroName")]
        public string Name { get; set; }

        [Column("macroScriptType")]
        public string ScriptType { get; set; }

        [Column("macroScriptAssembly")]
        public string ScriptAssembly { get; set; }

        [Column("macroXSLT")]
        public string Xslt { get; set; }

        [Column("macroCacheByPage")]
        public bool CacheByPage { get; set; }

        [Column("macroCachePersonalized")]
        public bool CachePersonalized { get; set; }

        [Column("macroDontRender")]
        public bool DontRender { get; set; }

        [Column("macroPython")]
        public string Python { get; set; }
    }
}