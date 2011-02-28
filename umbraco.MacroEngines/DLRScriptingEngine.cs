using System;
using System.Collections.Generic;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;
using umbraco.IO;
using umbraco.MacroEngines.Legacy.Scripting;

namespace umbraco.MacroEngines.Legacy
{
    public class DLRScriptingEngine : IMacroEngine
    {
        public string Name {
            get { return "Umbraco DLR Macro Engine"; }
        }

        IEnumerable<string> IMacroEngine.SupportedExtensions {
            get { return new[] { "py", "rb" }; }
        }

        public IEnumerable<string> SupportedUIExtensions {
            get { return new[] { "py", "rb" }; }
        }

        public Dictionary<string, IMacroGuiRendering> SupportedProperties {
            get { throw new NotImplementedException(); }
        }

        public bool Validate(string code, string tempFileName, INode currentPage, out string errorMessage) {
            throw new NotImplementedException();
        }

        public string Execute(MacroModel macro, INode currentPage) {
            var fileEnding = macro.ScriptName.Substring(macro.ScriptName.LastIndexOf('.')).Trim('.');
            var mse = MacroScriptEngine.LoadEngineByFileExtension(fileEnding);

            var vars = new SortedDictionary<string, object>();
            vars.Add("currentPage", currentPage);
            foreach (MacroPropertyModel prop in macro.Properties) {
                vars.Add(prop.Key, prop.Value);
            }
            mse.ScriptVariables = vars;

            return mse.ExecuteFile(IOHelper.MapPath(SystemDirectories.Python + "/" + macro.ScriptName));
        }
    }
}
