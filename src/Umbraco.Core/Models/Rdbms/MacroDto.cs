using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsMacro")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class MacroDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("macroUseInEditor")]
        [Constraint(Default = "0")]
        public bool UseInEditor { get; set; }

        [Column("macroRefreshRate")]
        [Constraint(Default = "0")]
        public int RefreshRate { get; set; }

        [Column("macroAlias")]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsMacroPropertyAlias")]
        public string Alias { get; set; }

        [Column("macroName")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Name { get; set; }

        [Column("macroScriptType")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ScriptType { get; set; }

        [Column("macroScriptAssembly")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ScriptAssembly { get; set; }

        [Column("macroXSLT")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Xslt { get; set; }

        [Column("macroCacheByPage")]
        [Constraint(Default = "1")]
        public bool CacheByPage { get; set; }

        [Column("macroCachePersonalized")]
        [Constraint(Default = "0")]
        public bool CachePersonalized { get; set; }

        [Column("macroDontRender")]
        [Constraint(Default = "0")]
        public bool DontRender { get; set; }

        [Column("macroPython")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Python { get; set; }

        [ResultColumn]
        public List<MacroPropertyDto> MacroPropertyDtos { get; set; }
    }
}