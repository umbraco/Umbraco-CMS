using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.macro
{

    [Serializable]
    public class MacroModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string MacroControlIdentifier { get; set; }
        public MacroTypes MacroType { get; set; }

        public string TypeAssembly { get; set; }
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

        public List<MacroPropertyModel> Properties { get; set; }

        public MacroModel()
        {
            Properties = new List<MacroPropertyModel>();
        }

        public MacroModel(Macro m)
        {
            Properties = new List<MacroPropertyModel>();
            if (m != null)
            {
                Id = m.Id;
                Name = m.Name;
                Alias = m.Alias;
                TypeAssembly = m.Assembly;
                TypeName = m.Type;
                Xslt = m.Xslt;
                ScriptName = m.ScriptingFile;
                CacheDuration = m.RefreshRate;
                CacheByPage = m.CacheByPage;
                CacheByMember = m.CachePersonalized;
                RenderInEditor = m.RenderContent;
                foreach (MacroProperty mp in m.Properties)
                {
                    Properties.Add(
                        new MacroPropertyModel(mp.Alias, string.Empty, mp.Type.Alias, mp.Type.BaseType));
                }
                MacroType = Macro.FindMacroType(Xslt, ScriptName, TypeName, TypeAssembly);
            }
        }

        public MacroModel(string name, string alias, string typeAssembly, string typeName, string xslt, string scriptName, int cacheDuration, bool cacheByPage, bool cacheByMember)
        {
            Name = name;
            Alias = alias;
            TypeAssembly = typeAssembly;
            TypeName = typeName;
            Xslt = xslt;
            ScriptName = scriptName;
            CacheDuration = cacheDuration;
            CacheByPage = cacheByPage;
            CacheByMember = cacheByMember;

            Properties = new List<MacroPropertyModel>();

            MacroType = Macro.FindMacroType(Xslt, ScriptName, TypeName, TypeAssembly);
        }
    }
}
