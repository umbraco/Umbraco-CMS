using System;
using System.Collections.Generic;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;
using umbraco.IO;
using umbraco.MacroEngines.Scripting;

namespace umbraco.MacroEngines
{
    public class DLRScriptingEngine : IMacroEngine
    {
        #region IMacroEngine Members

        public string Name
        {
            get { return "Umbraco DLR Macro Engine"; }
        }

        public List<string> SupportedExtensions
        {
            get
            {
                var exts = new List<string> {"py", "rb"};
                return exts;
            }
        }

        public Dictionary<string, IMacroGuiRendering> SupportedProperties
        {
            get { throw new NotImplementedException(); }
        }


        public bool Validate(string code, INode currentPage, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public string Execute(MacroModel macro, INode currentPage)
        {
            string fileEnding = macro.ScriptName.Substring(macro.ScriptName.LastIndexOf('.')).Trim('.');

            MacroScriptEngine mse = MacroScriptEngine.LoadEngineByFileExtension(fileEnding);

            var vars = new SortedDictionary<string, object> {{"currentPage", currentPage}};
            foreach (MacroPropertyModel prop in macro.Properties)
            {
                vars.Add(prop.Key, prop.Value);
            }
            mse.ScriptVariables = vars;

            return mse.ExecuteFile(IOHelper.MapPath(SystemDirectories.Python + "/" + macro.ScriptName));
        }

        #endregion
    }
}