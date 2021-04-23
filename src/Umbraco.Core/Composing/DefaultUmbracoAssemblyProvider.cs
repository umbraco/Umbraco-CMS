using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Composing
{
    /// <summary>
    /// Returns a list of scannable assemblies based on an entry point assembly and it's references
    /// </summary>
    /// <remarks>
    /// This will recursively search through the entry point's assemblies and Umbraco's core assemblies and their references
    /// to create a list of scannable assemblies based on whether they themselves or their transitive dependencies reference Umbraco core assemblies.
    /// </remarks>
    public class DefaultUmbracoAssemblyProvider : IAssemblyProvider
    {
        private readonly Assembly _entryPointAssembly;
        private readonly ILoggerFactory _loggerFactory;

        public DefaultUmbracoAssemblyProvider(Assembly entryPointAssembly, ILoggerFactory loggerFactory)
        {
            _entryPointAssembly = entryPointAssembly ?? throw new ArgumentNullException(nameof(entryPointAssembly));
            _loggerFactory = loggerFactory;
        }

        // TODO: It would be worth investigating a netcore3 version of this which would use
        // var allAssemblies = System.Runtime.Loader.AssemblyLoadContext.All.SelectMany(x => x.Assemblies);
        // that will still only resolve Assemblies that are already loaded but it would also make it possible to
        // query dynamically generated assemblies once they are added. It would also provide the ability to probe
        // assembly locations that are not in the same place as the entry point assemblies.

        public IEnumerable<Assembly> Assemblies
        {
            get
            {
                var finder = new FindAssembliesWithReferencesTo(new[] { _entryPointAssembly }, Constants.Composing.UmbracoCoreAssemblyNames, true, _loggerFactory);
                return finder.Find();
            }
        }
    }
}
