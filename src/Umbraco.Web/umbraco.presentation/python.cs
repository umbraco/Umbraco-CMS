using System;
using Umbraco.Core.IO;

namespace umbraco.scripting
{

    /// <summary>
    /// Something like a proxy to IronPython. Does some initial settings and calls.
    /// Maps IronPython's StandardOutput and StandardError to a simple string.
    /// </summary>
    public class python
    {
        protected internal static PythonEngine Engine;
        protected static System.IO.MemoryStream ms;
        protected static System.IO.StreamReader sr;
		protected internal static System.Collections.Hashtable scripts;

        static python()
        {
            Engine = new PythonEngine();

            initEnv();
			loadScripts();

            ms = new System.IO.MemoryStream();
            sr = new System.IO.StreamReader(ms);
            Engine.SetStandardOutput(ms);
            Engine.SetStandardError(ms);
        }

     
		/// <summary>
        /// To be able to import umbraco dll's we have to append the umbraco path to python.
        /// It should also be possible to import other python scripts from umbracos python folder.
        /// And finally to run run some custom init stuff the script site.py in umbraco's
        /// root folder will be executed.
        /// </summary>
        /// <returns></returns>
        private static void initEnv()
        {
            // Add umbracos bin folder to python's path
            string path = IOHelper.MapPath(SystemDirectories.Bin);
            Engine.AddToPath(path);

			// Add umbracos python folder to python's path
            path = IOHelper.MapPath(SystemDirectories.MacroScripts);
			Engine.AddToPath(path);

            // execute the site.py to do all the initial stuff
            string initFile = IOHelper.MapPath(SystemDirectories.Root + "/site.py");
            Engine.ExecuteFile(initFile);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private static void loadScripts()
		{
			scripts = new System.Collections.Hashtable();
            string path = IOHelper.MapPath(SystemDirectories.MacroScripts);
			System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);
			foreach (System.IO.FileInfo f in dir.GetFiles("*.py"))
			{
				if (!f.Name.EndsWith("_temp.py"))
				{
					try
					{
						scripts[f.FullName] = Engine.CompileFile(f.FullName );
					}
					catch
					{
						scripts[f.FullName] = Engine.Compile("print 'error in file " + f.Name + "'");
					}
				}
			}
		}

    
		/// <summary>
        /// Executes a python command like in console 
        /// </summary>
        /// <param name="expression">command to execute</param>
        /// <returns>returns standard out of executed command</returns>
        public static string execute(string expression)
        {
            if (!(expression == null))
            {
                string ret;
                ms.SetLength(0);
                ms.Flush();
                try
                {
                    Engine.Execute(expression);
                    ms.Position = 0;
                    ret = sr.ReadToEnd();
                }
                catch (Exception ex)
                {
                    ret = ex.Message;
                }
                return ret;
            }
            else
            {
                return string.Empty;
            }
        }

       
		/// <summary>
        /// Executes a python script like in console 
        /// </summary>
        /// <param name="file">absolute path to script</param>
        /// <returns>returns standard out of executed script</returns>
        public static string executeFile(string file)
        {
            if (System.IO.File.Exists(file))
            {
                string ret;
                ms.SetLength(0);
                ms.Flush();

				scripts[file].GetType().InvokeMember("Execute", System.Reflection.BindingFlags.InvokeMethod, null, scripts[file], null);

                ms.Position = 0;
                ret = sr.ReadToEnd();
                return ret;
            }
            else
            {
                return "The File " + file + " could not be found.";
            }
        }


		/// <summary>
		/// Compiles a python script and add it to umbraco's script collection.
		/// If compilation fails then an exception will be raised.
		/// </summary>
		/// <param name="file">absolute path to script</param>
		/// <returns></returns>
		public static void compileFile(string file)
		{
			loadScripts();
            scripts[file] = Engine.CompileFile(file);
		}

		
		/// <summary>
		/// Compiles a python script.
		/// If compilation fails then an exception will be raised.
		/// </summary>
		/// <param name="file">absolute path to script</param>
		/// <returns></returns>
		public static void tryCompile(string file)
		{
			Engine.CompileFile(file);
		}

    }


    /// <summary>
    /// The Class PythonEngine is just a wrapper for the real class IronPython.Hosting.PythonEngine
    /// in IronPython. In this manner we does not need a hard reference to the IronPython assembly.
    /// I've implemented only the methods i need for my purpose.
    /// </summary>
    public class PythonEngine
    {
        protected object Engine;

        public PythonEngine()
        {
            string path = IOHelper.MapPath(SystemDirectories.Bin + "/IronPython.dll");
            System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFile(path);
            System.Type EngineType = asm.GetType("IronPython.Hosting.PythonEngine");
            Engine = System.Activator.CreateInstance(EngineType);
        }

        public void AddToPath(string path)
        {
            Engine.GetType().InvokeMember("AddToPath", System.Reflection.BindingFlags.InvokeMethod, null, Engine, new object[] { path });
        }

        public void SetStandardOutput(System.IO.Stream stream)
        {
            Engine.GetType().InvokeMember("SetStandardOutput", System.Reflection.BindingFlags.InvokeMethod, null, Engine, new object[] { stream });
        }

        public void SetStandardError(System.IO.Stream stream)
        {
            Engine.GetType().InvokeMember("SetStandardError", System.Reflection.BindingFlags.InvokeMethod, null, Engine, new object[] { stream });
        }

        public void ExecuteFile(string FileName) 
        {
            Engine.GetType().InvokeMember("ExecuteFile", System.Reflection.BindingFlags.InvokeMethod, null, Engine, new object[] { FileName });
        }

        public void Execute(string ScriptCode) 
        {
            Engine.GetType().InvokeMember("Execute", System.Reflection.BindingFlags.InvokeMethod, null, Engine, new object[] { ScriptCode });
        }

		public Object CompileFile(string FileName) 
		{
			return Engine.GetType().InvokeMember("CompileFile", System.Reflection.BindingFlags.InvokeMethod, null, Engine, new object[] { FileName });
		}

		public Object Compile(string Expression) 
		{
			return Engine.GetType().InvokeMember("Compile", System.Reflection.BindingFlags.InvokeMethod, null, Engine, new object[] { Expression });
		}

		public Object CreateModule(string Modulename, bool publish) 
		{
			return Engine.GetType().InvokeMember("CreateModule", System.Reflection.BindingFlags.InvokeMethod, null, Engine, new object[] { Modulename, publish });
		}

        public System.Collections.IDictionary  Globals
        {
            set
            {
                Engine.GetType().InvokeMember("Globals", System.Reflection.BindingFlags.SetProperty, null, Engine, new object[] { value });  
            }
            get 
            {
                return (System.Collections.IDictionary)Engine.GetType().InvokeMember("Globals", System.Reflection.BindingFlags.GetProperty, null, Engine, null);  
            }
        }
    }
}