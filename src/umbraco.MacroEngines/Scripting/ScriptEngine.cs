using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace umbraco.MacroEngines.Scripting
{
    internal class MacroScriptEngine
    {
        ScriptRuntime m_runTime;
        ScriptEngine m_engine;
        ExceptionOperations m_exceptionOperations;
        SortedDictionary<string, object> m_inputVariables;
        SortedDictionary<string, object> m_outputVariables;
        string m_script;
        MemoryStream m_output;

        internal MacroScriptEngine() { }

        internal MacroScriptEngine(string scriptType)
        {
            loadRunTime();
            m_engine = m_runTime.GetEngine(scriptType);
            m_exceptionOperations = m_engine.GetService<ExceptionOperations>();
        }

        internal static MacroScriptEngine LoadEngineByFileExtension(string fileExtension)
        {
            MacroScriptEngine mse = new MacroScriptEngine();
            mse.loadRunTime();
            mse.m_engine = mse.m_runTime.GetEngineByFileExtension(fileExtension);
            mse.m_exceptionOperations = mse.m_engine.GetService<ExceptionOperations>();
            return mse;
        }

        internal static MacroScriptEngine GetEngineByType(string scriptType)
        {
            MacroScriptEngine mse = new MacroScriptEngine();
            mse.loadRunTime();
            mse.m_engine = mse.m_runTime.GetEngine(scriptType);
            mse.m_exceptionOperations = mse.m_engine.GetService<ExceptionOperations>();
            return mse;
        }

        private void loadRunTime()
        {
            m_output = new MemoryStream();
            m_runTime = ScriptRuntime.CreateFromConfiguration();

            m_runTime.IO.SetOutput(m_output, new StreamWriter(m_output));
            m_runTime.IO.SetErrorOutput(m_output, new StreamWriter(m_output));

            Assembly pluginsAssembly = Assembly.LoadFile(IO.IOHelper.MapPath(IO.SystemDirectories.Bin + "/umbraco.dll"));
            m_runTime.LoadAssembly(pluginsAssembly);

            m_runTime.LoadAssembly(typeof(String).Assembly);
            m_runTime.LoadAssembly(typeof(Uri).Assembly);
            m_runTime.LoadAssembly(typeof(umbraco.presentation.nodeFactory.Node).Assembly);
        }


        internal SortedDictionary<string, object> ScriptVariables
        {
            set { m_inputVariables = value; }
        }

        internal SortedDictionary<string, object> OutPutVariables
        {
            get { return m_outputVariables; }
        }

        internal string Script
        {
            set { m_script = value; }
        }

        internal ExceptionOperations ExceptionOperations
        {
            get { return m_exceptionOperations; }
        }

        internal string Evaluate()
        {
            //Create structures
            SourceCodeKind sc = SourceCodeKind.Expression;
            ScriptSource source = m_engine.CreateScriptSourceFromString(m_script, sc);
            ScriptScope scope = m_engine.CreateScope();

            //Fill input variables
            foreach (KeyValuePair<string, object> variable in m_inputVariables)
            {
                scope.SetVariable(variable.Key, variable.Value);
            }

            string result = "";

            try
            {
                object r = source.Execute(scope);

                if (r != null)
                    result = r.ToString();
            }
            catch (Exception e)
            {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, e.ToString());
                result = m_exceptionOperations.FormatException(e);
            }

            return result;
        }

        internal string Execute()
        {
            SourceCodeKind sc = SourceCodeKind.Statements;
            ScriptSource source = m_engine.CreateScriptSourceFromString(m_script, sc);
            ScriptScope scope = m_engine.CreateScope();

            //Fill input variables
            foreach (KeyValuePair<string, object> variable in m_inputVariables)
            {
                scope.SetVariable(variable.Key, variable.Value);
            }

            source.Execute(scope);

            m_outputVariables = new SortedDictionary<string, object>();
            foreach (string variable in scope.GetVariableNames())
            {
                m_outputVariables.Add(variable, scope.GetVariable(variable));
            }

            return ReadFromStream(m_output);
        }

        internal string ExecuteFile(string path)
        {
            string rbCode;

            // OpenText will strip the BOM and keep the Unicode intact
            using (var rdr = File.OpenText(path))
            {
                rbCode = rdr.ReadToEnd();
            }

            m_script = rbCode;
            return Execute();
        }

        internal SortedDictionary<string, object> _Execute()
        {
            //Create structures
            SourceCodeKind sc = SourceCodeKind.Statements;
            ScriptSource source = m_engine.CreateScriptSourceFromString(m_script, sc);
            ScriptScope scope = m_engine.CreateScope();

            //Fill input variables
            foreach (KeyValuePair<string, object> variable in m_inputVariables)
            {
                scope.SetVariable(variable.Key, variable.Value);
            }

            SortedDictionary<string, object> outputVariables = new SortedDictionary<string, object>();
            //Execute the script
            try
            {

                source.Execute(scope);
                //Recover variables
                foreach (string variable in scope.GetVariableNames())
                {
                    outputVariables.Add(variable, scope.GetVariable(variable));
                }
            }
            catch (Exception e)
            {
                string error = m_exceptionOperations.FormatException(e);
                //Do something with the pretty printed error
                throw;
            }
            return outputVariables;
        }

        private static string ReadFromStream(MemoryStream ms)
        {
            int length = (int)ms.Length;
            Byte[] bytes = new Byte[length];

            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(bytes, 0, (int)ms.Length);

            return Encoding.GetEncoding("utf-8").GetString(bytes, 0, (int)ms.Length);
        }
    }
}
