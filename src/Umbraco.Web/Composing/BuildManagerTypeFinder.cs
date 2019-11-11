using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Composing
{
    /// <summary>
    /// An implementation of TypeFinder that uses the BuildManager to resolve references for aspnet framework hosted websites
    /// </summary>
    /// <remarks>
    /// This finder will also try to resolve dynamic assemblies created from App_Code
    /// </remarks>
    internal class BuildManagerTypeFinder : TypeFinder, ITypeFinder
    {
        
        public BuildManagerTypeFinder(IIOHelper ioHelper, ILogger logger, ITypeFinderConfig typeFinderConfig = null) : base(logger, typeFinderConfig)
        {
            if (ioHelper == null) throw new ArgumentNullException(nameof(ioHelper));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _allAssemblies = new Lazy<HashSet<Assembly>>(() =>
            {
                var isHosted = ioHelper.IsHosted;
                try
                {
                    if (isHosted)
                    {
                        var assemblies = new HashSet<Assembly>(BuildManager.GetReferencedAssemblies().Cast<Assembly>());

                        //here we are trying to get the App_Code assembly
                        var fileExtensions = new[] { ".cs", ".vb" }; //only vb and cs files are supported
                        var appCodeFolder = new DirectoryInfo(ioHelper.MapPath(ioHelper.ResolveUrl("~/App_code")));
                        //check if the folder exists and if there are any files in it with the supported file extensions
                        if (appCodeFolder.Exists && fileExtensions.Any(x => appCodeFolder.GetFiles("*" + x).Any()))
                        {
                            try
                            {
                                var appCodeAssembly = Assembly.Load("App_Code");
                                if (assemblies.Contains(appCodeAssembly) == false) // BuildManager will find App_Code already
                                    assemblies.Add(appCodeAssembly);
                            }
                            catch (FileNotFoundException ex)
                            {
                                //this will occur if it cannot load the assembly
                                logger.Error(typeof(TypeFinder), ex, "Could not load assembly App_Code");
                            }
                        }
                    }
                }
                catch (InvalidOperationException e)
                {
                    if (e.InnerException is SecurityException == false)
                        throw;
                }

                // Not hosted, just use the default implementation
                return new HashSet<Assembly>(base.AssembliesToScan);
            });
        }

        private readonly Lazy<HashSet<Assembly>> _allAssemblies;

        /// <summary>
        /// Explicitly implement and return result from BuildManager
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Type ITypeFinder.GetTypeByName (string name) => BuildManager.GetType(name, false);

        /// <summary>
        /// Explicitly implement and return result from BuildManager
        /// </summary>
        IEnumerable<Assembly> ITypeFinder.AssembliesToScan => _allAssemblies.Value;

        /// <summary>
        /// TypeFinder config via appSettings
        /// </summary>
        internal class TypeFinderConfig : ITypeFinderConfig
        {
            private IEnumerable<string> _assembliesAcceptingLoadExceptions;
            public IEnumerable<string> AssembliesAcceptingLoadExceptions
            {
                get
                {
                    if (_assembliesAcceptingLoadExceptions != null)
                        return _assembliesAcceptingLoadExceptions;

                    var s = ConfigurationManager.AppSettings[Constants.AppSettings.AssembliesAcceptingLoadExceptions];
                    return _assembliesAcceptingLoadExceptions = string.IsNullOrWhiteSpace(s)
                        ? Array.Empty<string>()
                        : s.Split(',').Select(x => x.Trim()).ToArray();
                }
            }
        }
    }
}
