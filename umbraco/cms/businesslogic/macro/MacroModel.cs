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
        public string CacheIdenitifier { get; set; }

        public List<MacroPropertyModel> Properties { get; set; }

        public MacroModel()
        {
            Properties = new List<MacroPropertyModel>();
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

    [Serializable]
    public class MacroPropertyModel
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public MacroPropertyModel()
        {

        }

        public MacroPropertyModel(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    public enum MacroTypes
    {
        XSLT = 1,
        CustomControl = 2,
        UserControl = 3,
        Unknown = 4,
        Python = 5,
        Script = 6
    }


}
