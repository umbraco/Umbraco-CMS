using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Resolves assemblies that reference one of the specified "targetAssemblies" either directly or transitively.
    /// </summary>
    /// <remarks>
    /// Borrowed and modified from https://github.com/dotnet/aspnetcore-tooling/blob/master/src/Razor/src/Microsoft.NET.Sdk.Razor/ReferenceResolver.cs
    /// </remarks>
    internal class ReferenceResolver
    {
        private readonly HashSet<string> _umbracoAssemblies;
        private readonly IReadOnlyList<Assembly> _assemblies;
        private readonly Dictionary<Assembly, Classification> _classifications;
        private readonly List<Assembly> _lookup = new List<Assembly>();

        public ReferenceResolver(IReadOnlyList<string> targetAssemblies, IReadOnlyList<Assembly> entryPointAssemblies)
        {
            _umbracoAssemblies = new HashSet<string>(targetAssemblies, StringComparer.Ordinal);
            _assemblies = entryPointAssemblies;
            _classifications = new Dictionary<Assembly, Classification>();

            foreach (var item in entryPointAssemblies)
            {
                _lookup.Add(item);
            }
        }

        /// <summary>
        /// Returns a list of assemblies that directly reference or transitively reference the targetAssemblies
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This includes all assemblies in the same location as the entry point assemblies
        /// </remarks>
        public IEnumerable<Assembly> ResolveAssemblies()
        {
            var applicationParts = new List<Assembly>();

            var assemblies = new HashSet<Assembly>(_assemblies);

            // Get the unique directories of the assemblies
            var assemblyLocations = GetAssemblyLocations(assemblies).ToList();

            // Load in each assembly in the directory of the entry assembly to be included in the search
            // for Umbraco dependencies/transitive dependencies
            foreach(var location in assemblyLocations)
            {
                var dir = Path.GetDirectoryName(location);
                
                foreach(var dll in Directory.EnumerateFiles(dir, "*.dll"))
                {
                    var assemblyName = AssemblyName.GetAssemblyName(dll);

                    // don't include if this is excluded
                    if (TypeFinder.KnownAssemblyExclusionFilter.Any(f => assemblyName.FullName.StartsWith(f, StringComparison.InvariantCultureIgnoreCase)))
                        continue;

                    // don't include this item if it's Umbraco
                    // TODO: We should maybe pass an explicit list of these names in?
                    if (assemblyName.FullName.StartsWith("Umbraco."))
                        continue;

                    var assembly = Assembly.Load(assemblyName);
                    assemblies.Add(assembly);
                }
            }

            foreach (var item in assemblies)
            {
                var classification = Resolve(item);
                if (classification == Classification.ReferencesUmbraco || classification == Classification.IsUmbraco)
                {
                    applicationParts.Add(item);
                }
            }

            return applicationParts;
        }


        private IEnumerable<string> GetAssemblyLocations(IEnumerable<Assembly> assemblies)
        {
            return assemblies.Select(x => GetAssemblyLocation(x).ToLowerInvariant()).Distinct();
        }

        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Core/src/ApplicationParts/RelatedAssemblyAttribute.cs
        private string GetAssemblyLocation(Assembly assembly)
        {
            if (Uri.TryCreate(assembly.CodeBase, UriKind.Absolute, out var result) &&
                result.IsFile && string.IsNullOrWhiteSpace(result.Fragment))
            {
                return result.LocalPath;
            }

            return assembly.Location;
        }

        private Classification Resolve(Assembly assemblyItem)
        {
            if (_classifications.TryGetValue(assemblyItem, out var classification))
            {
                return classification;
            }

            // Initialize the dictionary with a value to short-circuit recursive references.
            classification = Classification.Unknown;
            _classifications[assemblyItem] = classification;

            if (_umbracoAssemblies.Contains(assemblyItem.GetName().Name))
            {
                classification = Classification.IsUmbraco;
            }
            else
            {
                classification = Classification.DoesNotReferenceUmbraco;
                foreach (var reference in GetReferences(assemblyItem))
                {
                    // recurse
                    var referenceClassification = Resolve(reference);

                    if (referenceClassification == Classification.IsUmbraco || referenceClassification == Classification.ReferencesUmbraco)
                    {
                        classification = Classification.ReferencesUmbraco;
                        break;
                    }
                }
            }

            Debug.Assert(classification != Classification.Unknown);
            _classifications[assemblyItem] = classification;
            return classification;
        }

        protected virtual IEnumerable<Assembly> GetReferences(Assembly assembly)
        {            
            foreach (var referenceName in assembly.GetReferencedAssemblies())
            {
                // don't include if this is excluded
                if (TypeFinder.KnownAssemblyExclusionFilter.Any(f => referenceName.FullName.StartsWith(f, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                var reference = Assembly.Load(referenceName);
                if (!_lookup.Contains(reference))
                {
                    // A dependency references an item that isn't referenced by this project.
                    // We'll construct an item for so that we can calculate the classification based on it's name.

                    _lookup.Add(reference);

                    yield return reference;
                }                
            }
        }

        protected enum Classification
        {
            Unknown,
            DoesNotReferenceUmbraco,
            ReferencesUmbraco,
            IsUmbraco,
        }
    }
}
