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
        private static object locker = new object();

        public MacroEngineFactory()
        {
            Initialize();
        }

        protected static void Initialize()
        {
        	var typeFinder = new Umbraco.Core.TypeFinder2();
			var types = typeFinder.FindClassesOfType<IMacroEngine>();
            GetEngines(types);
        }

        private static void GetEngines(IEnumerable<Type> types)
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
                        lock (locker)
                        {
                            if (!m_engines.ContainsKey(typeInstance.Name))
                                m_engines.Add(typeInstance.Name, t);
                        }
                    }
                    catch (Exception ee)
                    {
                        BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Error, -1, "Can't import MacroEngine '" + t.FullName + "': " + ee);
                    }
                }
            }
        }

        public static IEnumerable<MacroEngineLanguage> GetSupportedLanguages() {
            var languages = new List<MacroEngineLanguage>();
            foreach(var engine in GetAll()) {
                foreach(string lang in engine.SupportedExtensions)
                    if (languages.Find(t => t.Extension == lang) == null)
                        languages.Add(new MacroEngineLanguage(lang, engine.Name));
            }
            return languages;
        }

        public static IEnumerable<MacroEngineLanguage> GetSupportedUILanguages() {
            var languages = new List<MacroEngineLanguage>();
            foreach (var engine in GetAll()) {
                foreach (string lang in engine.SupportedUIExtensions)
                    if (languages.Find(t => t.Extension == lang) == null)
                        languages.Add(new MacroEngineLanguage(lang, engine.Name));
            }
            return languages;
        }

        public static List<IMacroEngine> GetAll()
        {

            if (m_allEngines.Count == 0)
            {
                Initialize();
                foreach (string name in m_engines.Keys) {
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

    public class MacroEngineLanguage
    {
        public string Extension { get; set; }
        public string EngineName { get; set; }
        public MacroEngineLanguage()
        {
            
        }

        public MacroEngineLanguage(string extension, string engineName)
        {
            Extension = extension;
            EngineName = engineName;
        }
    }
}
