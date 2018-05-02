using System.Collections.Generic;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Web.Macros
{
    public class MacroModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Alias { get; set; }

        public string MacroControlIdentifier { get; set; }

        public MacroTypes MacroType { get; set; }

        public string MacroSource { get; set; }

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
            MacroType = macro.MacroType;
            MacroSource = macro.MacroSource;
            CacheDuration = macro.CacheDuration;
            CacheByPage = macro.CacheByPage;
            CacheByMember = macro.CacheByMember;
            RenderInEditor = macro.UseInEditor;

            foreach (var prop in macro.Properties)
                Properties.Add(new MacroPropertyModel(prop.Alias, string.Empty, prop.EditorAlias));

            MacroType = macro.MacroType;
        }
    }
}
