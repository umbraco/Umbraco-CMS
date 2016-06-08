using System.Collections.Generic;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Web.Macros
{
    public class MacroModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Alias { get; set; }

        public string MacroControlIdentifier { get; set; }

        public MacroTypes MacroType { get; set; }

        // that one was for CustomControls which are gone in v8
        //public string TypeAssembly { get; set; }

        public string TypeName { get; set; }

        public string Xslt { get; set; }

        public string ScriptName { get; set; }

        public string ScriptCode { get; set; }

        public string ScriptLanguage { get; set; }

        public int CacheDuration { get; set; }

        public bool CacheByPage { get; set; }

        public bool CacheByMember { get; set; }

        public bool RenderInEditor { get; set; }

        public string CacheIdentifier { get; set; }

        public List<MacroPropertyModel> Properties { get; } = new List<MacroPropertyModel>();

        public MacroModel()
        { }

        public MacroModel(IMacro macro)
        {
            if (macro == null) return;

            Id = macro.Id;
            Name = macro.Name;
            Alias = macro.Alias;
            //TypeAssembly = macro.ControlAssembly;
            TypeName = macro.ControlType;
            Xslt = macro.XsltPath;
            ScriptName = macro.ScriptPath;
            CacheDuration = macro.CacheDuration;
            CacheByPage = macro.CacheByPage;
            CacheByMember = macro.CacheByMember;
            RenderInEditor = macro.UseInEditor;

            foreach (var prop in macro.Properties)
                Properties.Add(new MacroPropertyModel(prop.Alias, string.Empty, prop.EditorAlias));

            // can convert enums
            MacroType = Core.Services.MacroService.GetMacroType(macro);
        }
    }
}
