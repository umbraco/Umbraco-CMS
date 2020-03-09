using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IReadOnlyList<Assembly> _assemblyItems;
        private readonly Dictionary<Assembly, Classification> _classifications;
        private readonly List<Assembly> _lookup = new List<Assembly>();

        public ReferenceResolver(IReadOnlyList<string> targetAssemblies, IReadOnlyList<Assembly> assemblyItems)
        {
            _umbracoAssemblies = new HashSet<string>(targetAssemblies, StringComparer.Ordinal);
            _assemblyItems = assemblyItems;
            _classifications = new Dictionary<Assembly, Classification>();

            foreach (var item in assemblyItems)
            {
                _lookup.Add(item);
            }
        }

        public IEnumerable<Assembly> ResolveAssemblies()
        {
            var applicationParts = new List<Assembly>();

            foreach (var item in _assemblyItems)
            {
                var classification = Resolve(item);
                if (classification == Classification.ReferencesUmbraco || classification == Classification.IsUmbraco)
                {
                    applicationParts.Add(item);
                }
            }

            return applicationParts;
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
                if (TypeFinder.KnownAssemblyExclusionFilter.Any(f => referenceName.FullName.StartsWith(f)))
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
