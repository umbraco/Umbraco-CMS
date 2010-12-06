using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic.Utils;
using umbraco.interfaces;

namespace umbraco.cms.businesslogic.macro
{
    public class MacroEngineFactory
    {
        private static readonly Dictionary<string, Type> m_engines = new Dictionary<string, Type>();
        private static readonly List<IMacroEngine> m_allEngines = new List<IMacroEngine>();
        public MacroEngineFactory()
        {
            Initialize();
        }

        protected static void Initialize()
        {
            List<Type> types = TypeFinder.FindClassesOfType<IMacroEngine>();
            getEngines(types);
        }

        private static void getEngines(List<Type> types)
        {
            foreach (Type t in types)
            {
                IMacroEngine typeInstance = null;
                try
                {
                    if (t.IsVisible)
                    {
                        typeInstance = Activator.CreateInstance(t) as IMacroEngine;
                    }
                }
                catch { }
                if (typeInstance != null)
                {
                    try
                    {
                        m_engines.Add(typeInstance.Name, t);
                    }
                    catch (Exception ee)
                    {
                        BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Error, -1, "Can't import MacroEngine '" + t.FullName + "': " + ee);
                    }
                }
            }
        }

        public static List<IMacroEngine> GetAll()
        {

            if (m_allEngines.Count == 0)
            {
                Initialize();
                foreach (string name in m_engines.Keys)
                {
                    m_allEngines.Add(GetEngine(name));
                }
            }

            return m_allEngines;
        }

        public static IMacroEngine GetEngine(string name)
        {
            if (m_engines.ContainsKey(name))
            {
                var newObject = Activator.CreateInstance(m_engines[name]) as IMacroEngine;
                return newObject;
            }

            return null;
        }

        public static IMacroEngine GetByFilename(string filename)
        {
            if (filename.Contains("."))
            {
                string extension = filename.Substring(filename.LastIndexOf(".") + 1);
                return GetByExtension(extension);
            }

            throw new MacroEngineException(string.Format("No MacroEngine matches the file with extension '{0}'", filename));
        }

        public static IMacroEngine GetByExtension(string extension)
        {
            IMacroEngine engine =
                GetAll().Find(t => t.SupportedExtensions.Contains(extension));
            if (engine != null)
            {
                return engine;
            }

            throw new MacroEngineException(string.Format("No MacroEngine found for extension '{0}'", extension));
        }
    }

    public class MacroEngineException : Exception
    {
        public MacroEngineException() : base() { }

        public MacroEngineException(string msg)
            : base(msg)
        {

        }
    }
}
