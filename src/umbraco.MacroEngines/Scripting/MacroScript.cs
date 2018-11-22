using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Scripting.Hosting;

namespace umbraco.MacroEngines.Scripting
{
    public class MacroScript {

        public static string Execute(string script, string scriptType, Hashtable variables) {
            var mse = new MacroScriptEngine(scriptType);
            mse.ScriptVariables = ConvertHashTable(variables);
            mse.Script = script;
            return mse.Execute();
        }

        public static string Evaluate(string script, string scriptType, Hashtable variables)
        {
            var mse = new MacroScriptEngine(scriptType);
            mse.ScriptVariables = ConvertHashTable(variables);
            mse.Script = script;
            return mse.Evaluate();
        }

        public static string ExecuteFile(string path, Hashtable variables) {
            var fileEnding = path.Substring(path.LastIndexOf('.')).Trim('.');
            var mse = MacroScriptEngine.LoadEngineByFileExtension(fileEnding);
            mse.ScriptVariables = ConvertHashTable(variables);
            return mse.ExecuteFile(path);
        }

        //friendly helpers....
        public static string ExecutePython(string script, Hashtable variables) {
            return Execute(script, "python", variables);
        }

        public static string ExecuteRuby(string script, Hashtable variables) {
            return Execute(script, "ruby", variables);
        }

        private static SortedDictionary<string, object> ConvertHashTable(Hashtable ht) {
            var retval = new SortedDictionary<string, object>();
            foreach (DictionaryEntry de in ht) {
                retval.Add(de.Key.ToString(), de.Value);
            }
            return retval;
        }

        public static List<LanguageSetup> GetAvailableLanguages() {
            return ScriptRuntimeSetup.ReadConfiguration().LanguageSetups.ToList();
        }
    }
}
