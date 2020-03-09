using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// lazily load a reference to all local assemblies and gac assemblies
    /// </summary>
    /// <remarks>
    /// This is a modified version of: http://www.dominicpettifer.co.uk/Blog/44/how-to-get-a-reference-to-all-assemblies-in-the--bin-folder
    /// 
    /// We do this because we cannot use AppDomain.Current.GetAssemblies() as this will return only assemblies that have been
    /// loaded in the CLR, not all assemblies.
    /// See these threads:
    /// http://issues.umbraco.org/issue/U5-198
    /// http://stackoverflow.com/questions/3552223/asp-net-appdomain-currentdomain-getassemblies-assemblies-missing-after-app
    /// http://stackoverflow.com/questions/2477787/difference-between-appdomain-getassemblies-and-buildmanager-getreferencedassembl
    /// </remarks>
    public class BruteForceAssemblyProvider : IAssemblyProvider
    {
        public BruteForceAssemblyProvider()
        {
            _allAssemblies = new Lazy<HashSet<Assembly>>(() =>
            {
                HashSet<Assembly> assemblies = null;
                try
                {
                    //NOTE: we cannot use AppDomain.CurrentDomain.GetAssemblies() because this only returns assemblies that have
                    // already been loaded in to the app domain, instead we will look directly into the bin folder and load each one.
                    var binFolder = GetRootDirectorySafe();
                    var binAssemblyFiles = Directory.GetFiles(binFolder, "*.dll", SearchOption.TopDirectoryOnly).ToList();                    
                    assemblies = new HashSet<Assembly>();
                    foreach (var a in binAssemblyFiles)
                    {
                        try
                        {
                            var assName = AssemblyName.GetAssemblyName(a);
                            var ass = Assembly.Load(assName);
                            assemblies.Add(ass);
                        }
                        catch (Exception e)
                        {
                            if (e is SecurityException || e is BadImageFormatException)
                            {
                                //swallow these exceptions
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    //Since we are only loading in the /bin assemblies above, we will also load in anything that's already loaded (which will include gac items)
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        assemblies.Add(a);
                    }
                }
                catch (InvalidOperationException e)
                {
                    if (e.InnerException is SecurityException == false)
                        throw;
                }

                return assemblies;
            });
        }

        private readonly Lazy<HashSet<Assembly>> _allAssemblies;
        private string _rootDir = string.Empty;

        public IEnumerable<Assembly> Assemblies => _allAssemblies.Value;

        // FIXME - this is only an interim change, once the IIOHelper stuff is merged we should use IIOHelper here
        private string GetRootDirectorySafe()
        {
            if (string.IsNullOrEmpty(_rootDir) == false)
            {
                return _rootDir;
            }

            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            var baseDirectory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(baseDirectory))
                throw new PanicException("No root directory could be resolved.");

            _rootDir = baseDirectory.Contains("bin")
                ? baseDirectory.Substring(0, baseDirectory.LastIndexOf("bin", StringComparison.OrdinalIgnoreCase) - 1)
                : baseDirectory;

            return _rootDir;
        }
    }
}
