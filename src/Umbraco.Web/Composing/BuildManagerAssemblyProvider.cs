using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Web.Compilation;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Composing
{
    /// <summary>
    /// Uses the BuildManager to provide a list of assemblies to scan
    /// </summary>
    internal class BuildManagerAssemblyProvider : BruteForceAssemblyProvider, IAssemblyProvider
    {
        private readonly Lazy<HashSet<Assembly>> _allAssemblies;

        public BuildManagerAssemblyProvider(IIOHelper ioHelper,
            IHostingEnvironment hostingEnvironment,
            ILogger logger)
        {
            _allAssemblies = new Lazy<HashSet<Assembly>>(() =>
            {
                var isHosted = hostingEnvironment.IsHosted;
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

                        return assemblies;
                    }
                }
                catch (InvalidOperationException e)
                {
                    if (e.InnerException is SecurityException == false)
                        throw;
                }

                // Not hosted, just use the default implementation
                return new HashSet<Assembly>(base.Assemblies);
            });
        }

        IEnumerable<Assembly> IAssemblyProvider.Assemblies => _allAssemblies.Value;
    }
}
