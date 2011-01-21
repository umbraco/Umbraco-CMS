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

        public virtual string Name
        {
            get { return "Umbraco DLR Macro Engine"; }
        }

        public virtual List<string> SupportedExtensions
        {
            get
            {
                var exts = new List<string> { "py", "rb" };
                return exts;
            }
        }

        public virtual Dictionary<string, IMacroGuiRendering> SupportedProperties
        {
            get { throw new NotImplementedException(); }
        }


        public virtual bool Validate(string code, string filePath, INode currentPage, out string errorMessage) {
            errorMessage = string.Empty;
            return true;
        }

        public virtual string Execute(MacroModel macro, INode currentPage)
        {
            string fileEnding = macro.ScriptName.Substring(macro.ScriptName.LastIndexOf('.')).Trim('.');

            MacroScriptEngine mse = MacroScriptEngine.LoadEngineByFileExtension(fileEnding);

            var vars = new SortedDictionary<string, object>
                           {
                               {"currentPage", new DynamicNode(currentPage)}
                           };
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