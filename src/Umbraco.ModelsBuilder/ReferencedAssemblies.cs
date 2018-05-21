using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;
using Microsoft.CodeAnalysis;
using Umbraco.Core;

namespace Umbraco.ModelsBuilder
{
    internal static class ReferencedAssemblies
    {
        private static readonly Lazy<IEnumerable<string>> LazyLocations;
        private static readonly Lazy<IEnumerable<PortableExecutableReference>> LazyReferences;

        static ReferencedAssemblies()
        {
            LazyLocations = new Lazy<IEnumerable<string>>(() => HostingEnvironment.IsHosted
                ? GetAllReferencedAssembliesLocationFromBuildManager()
                : GetAllReferencedAssembliesFromDomain());

            LazyReferences = new Lazy<IEnumerable<PortableExecutableReference>>(() => Locations
                .Select(x => MetadataReference.CreateFromFile(x))
                .ToArray());
        }

        /// <summary>
        /// Gets the assembly locations of all the referenced assemblies, that
        /// are not dynamic, and have a non-null nor empty location.
        /// </summary>
        public static IEnumerable<string> Locations => LazyLocations.Value;

        /// <summary>
        /// Gets the metadata reference of all the referenced assemblies.
        /// </summary>
        public static IEnumerable<PortableExecutableReference> References => LazyReferences.Value;

        // hosted, get referenced assemblies from the BuildManader and filter
        private static IEnumerable<string> GetAllReferencedAssembliesLocationFromBuildManager()
        {
            return BuildManager.GetReferencedAssemblies()
                .Cast<Assembly>()
                .Where(x => !x.IsDynamic && !x.Location.IsNullOrWhiteSpace())
                .Select(x => x.Location)
                .And(typeof(ReferencedAssemblies).Assembly.Location) // always include ourselves
                .Distinct()
                .ToList();
        }

        // non-hosted, do our best
        private static IEnumerable<string> GetAllReferencedAssembliesFromDomain()
        {
            //TODO: This method has bugs since I've been stuck in an infinite loop with it, though this shouldn't
            // execute while in the web application anyways.

            var assemblies = new List<Assembly>();
            var tmp1 = new List<Assembly>();
            var failed = new List<AssemblyName>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.IsDynamic == false)
                .Where(x => !string.IsNullOrWhiteSpace(x.Location))) // though... IsDynamic should be enough?
            {
                assemblies.Add(assembly);
                tmp1.Add(assembly);
            }

            // fixme - AssemblyUtility questions
            // - should we also load everything that's in the same directory?
            // - do we want to load in the current app domain?
            // - if this runs within Umbraco then we have already loaded them all?

            while (tmp1.Count > 0)
            {
                var tmp2 = tmp1
                    .SelectMany(x => x.GetReferencedAssemblies())
                    .Distinct()
                    .Where(x => assemblies.All(xx => x.FullName != xx.FullName)) // we don't have it already
                    .Where(x => failed.All(xx => x.FullName != xx.FullName)) // it hasn't failed already
                    .ToArray();
                tmp1.Clear();
                foreach (var assemblyName in tmp2)
                {
                    try
                    {
                        var assembly = AppDomain.CurrentDomain.Load(assemblyName);
                        assemblies.Add(assembly);
                        tmp1.Add(assembly);
                    }
                    catch
                    {
                        failed.Add(assemblyName);
                    }
                }
            }
            return assemblies.Select(x => x.Location).Distinct();
        }


        // ----

        private static IEnumerable<Assembly> GetDeepReferencedAssemblies(Assembly assembly)
        {
            var visiting = new Stack<Assembly>();
            var visited = new HashSet<Assembly>();

            visiting.Push(assembly);
            visited.Add(assembly);
            while (visiting.Count > 0)
            {
                var visAsm = visiting.Pop();
                foreach (var refAsm in visAsm.GetReferencedAssemblies()
                    .Select(TryLoad)
                    .Where(x => x != null && visited.Contains(x) == false))
                {
                    yield return refAsm;
                    visiting.Push(refAsm);
                    visited.Add(refAsm);
                }
            }
        }

        private static Assembly TryLoad(AssemblyName name)
        {
            try
            {
                return AppDomain.CurrentDomain.Load(name);
            }
            catch (Exception)
            {
                //Console.WriteLine(name);
                return null;
            }
        }

    }
}
