using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Umbraco.Core;
using umbraco.BusinessLogic.Utils;
using umbraco.interfaces;

namespace umbraco.cms.businesslogic.macro
{

	//TODO: This class needs to be changed to use the new MultipleResolverBase, doing this will require migrating and cleaning up
	// a bunch of types so I have left it existing here under legacy code for now. The IMacroEngine interface also requires fixing
	// considering the new macro types of SurfaceControllers.

    public class MacroEngineFactory
    {
        private static readonly List<IMacroEngine> AllEngines = new List<IMacroEngine>();
    	private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
    	private static volatile bool _isInitialized = false;

        public MacroEngineFactory()
        {
			EnsureInitialize();
        }

		internal static void EnsureInitialize()
		{
			using (var lck = new UpgradeableReadLock(Lock))
			{
                if (_isInitialized)
                    return;

                lck.UpgradeToWriteLock();

				AllEngines.Clear();

				AllEngines.AddRange(
					PluginManager.Current.CreateInstances<IMacroEngine>(
						PluginManager.Current.ResolveMacroEngines()));
				
				_isInitialized = true;
			}
		}

		[Obsolete("Use EnsureInitialize method instead")]
        protected static void Initialize()
        {
        	EnsureInitialize();
        }

		/// <summary>
		/// Returns a collectino of MacroEngineLanguage objects, each of which describes a file extension and an associated macro engine
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Until the macro engines are rewritten, this method explicitly ignores the PartialViewMacroEngine because this method 
		/// is essentially just used for any macro engine that stores it's files in the ~/macroScripts folder where file extensions
		/// cannot overlap.
		/// </remarks>
		[Obsolete("This method is not used and will be removed from the codebase in the future")]
        public static IEnumerable<MacroEngineLanguage> GetSupportedLanguages() 
		{
            var languages = new List<MacroEngineLanguage>();
            foreach(var engine in GetAll()) 
			{
                foreach (string lang in engine.SupportedExtensions)
                {
					if (languages.Find(t => t.Extension == lang) == null)
					{
						languages.Add(new MacroEngineLanguage(lang, engine.Name));
					}
                }
            }
            return languages;
        }

		/// <summary>
		/// Returns a collectino of MacroEngineLanguage objects, each of which describes a file extension and an associated macro engine that
		/// supports file extension lookups.
		/// </summary>
		/// <returns></returns>		
		/// <remarks>
		/// The PartialViewMacroEngine will never be returned in these results because it does not support searching by file extensions. See
		/// the notes in the PartialViewMacroEngine regarding this.
		/// </remarks>
        public static IEnumerable<MacroEngineLanguage> GetSupportedUILanguages()
		{
			var languages = new List<MacroEngineLanguage>();
			foreach (var engine in GetAll())
			{
				foreach (string lang in engine.SupportedUIExtensions)
				{
					if (languages.All(t => t.Extension != lang))
					{
						languages.Add(new MacroEngineLanguage(lang, engine.Name));
					}
				}
			}
			return languages.OrderBy(s => s.Extension);
		}		

        public static List<IMacroEngine> GetAll()
        {
			EnsureInitialize();            
            return AllEngines;
        }

        public static IMacroEngine GetEngine(string name)
        {
        	EnsureInitialize();
        	var engine = AllEngines.FirstOrDefault(x => x.Name == name);
        	return engine;            
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
            var engine = GetAll().Find(t => t.SupportedExtensions.Contains(extension));
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
