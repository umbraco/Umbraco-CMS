﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Composing
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
        private readonly ILogger _logger;
        public ReferenceResolver(IReadOnlyList<string> targetAssemblies, IReadOnlyList<Assembly> entryPointAssemblies, ILogger<ReferenceResolver> logger)
        {
            _umbracoAssemblies = new HashSet<string>(targetAssemblies, StringComparer.Ordinal);
            _assemblies = entryPointAssemblies;
            _logger = logger;
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
            var assemblyLocations = GetAssemblyFolders(assemblies).ToList();

            // Load in each assembly in the directory of the entry assembly to be included in the search
            // for Umbraco dependencies/transitive dependencies
            foreach(var dir in assemblyLocations)
            {
                foreach(var dll in Directory.EnumerateFiles(dir, "*.dll"))
                {
                    AssemblyName assemblyName = null;
                    try
                    {
                        assemblyName = AssemblyName.GetAssemblyName(dll);
                    }
                    catch (BadImageFormatException e)
                    {
                        _logger.LogDebug(e, "Could not load {dll} for type scanning, skipping", dll);
                    }
                    catch (SecurityException e)
                    {
                        _logger.LogError(e, "Could not access {dll} for type scanning due to a security problem", dll);
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation(e, "Error: could not load {dll} for type scanning", dll);
                    }

                    if (assemblyName != null)
                    {
                        // don't include if this is excluded
                        if (TypeFinder.KnownAssemblyExclusionFilter.Any(f =>
                            assemblyName.FullName.StartsWith(f, StringComparison.InvariantCultureIgnoreCase)))
                            continue;

                        // don't include this item if it's Umbraco Core
                        if (Constants.Composing.UmbracoCoreAssemblyNames.Any(x=>assemblyName.FullName.StartsWith(x) || assemblyName.Name.EndsWith(".Views")))
                            continue;

                        var assembly = Assembly.Load(assemblyName);
                        assemblies.Add(assembly);
                    }
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


        private IEnumerable<string> GetAssemblyFolders(IEnumerable<Assembly> assemblies)
        {
            return assemblies.Select(x => Path.GetDirectoryName(GetAssemblyLocation(x))).Distinct();
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

        private Classification Resolve(Assembly assembly)
        {
            if (_classifications.TryGetValue(assembly, out var classification))
            {
                return classification;
            }

            // Initialize the dictionary with a value to short-circuit recursive references.
            classification = Classification.Unknown;
            _classifications[assembly] = classification;

            if (TypeFinder.KnownAssemblyExclusionFilter.Any(f => assembly.FullName.StartsWith(f, StringComparison.InvariantCultureIgnoreCase)))
            {
                // if its part of the filter it doesn't reference umbraco
                classification = Classification.DoesNotReferenceUmbraco;
            }
            else if (_umbracoAssemblies.Contains(assembly.GetName().Name))
            {
                classification = Classification.IsUmbraco;
            }
            else
            {
                classification = Classification.DoesNotReferenceUmbraco;
                foreach (var reference in GetReferences(assembly))
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
            _classifications[assembly] = classification;
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
                    // We'll add this reference so that we can calculate the classification.

                    _lookup.Add(reference);
                }
                yield return reference;
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
