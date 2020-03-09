using System;
using System.Collections.Generic;
using System.Reflection;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Returns a list of scannable assemblies based on an entry point assembly and it's references
    /// </summary>
    /// <remarks>
    /// This will recursively search through the entry point's assemblies and Umbraco's core assemblies (Core/Web) and their references
    /// to create a list of scannable assemblies based on whether they themselves or their transitive dependencies reference Umbraco core assemblies.
    /// </remarks>
    public class DefaultUmbracoAssemblyProvider : IAssemblyProvider
    {
        private readonly Assembly _entryPointAssembly;
        private static readonly string[] UmbracoCoreAssemblyNames = new[] { "Umbraco.Core", "Umbraco.Web" };

        public DefaultUmbracoAssemblyProvider(Assembly entryPointAssembly)
        {
            _entryPointAssembly = entryPointAssembly ?? throw new ArgumentNullException(nameof(entryPointAssembly));
        }

        public IEnumerable<Assembly> Assemblies
        {
            get
            {
                var finder = new FindAssembliesWithReferencesTo(new[] { _entryPointAssembly }, UmbracoCoreAssemblyNames, true);
                foreach(var found in finder.Find())
                {
                    yield return found;
                }
            }
        }
    }
}
