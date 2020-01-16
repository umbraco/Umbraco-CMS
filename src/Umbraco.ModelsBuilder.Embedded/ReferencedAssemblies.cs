using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;
using Umbraco.Core;

namespace Umbraco.ModelsBuilder.Embedded
{
    internal static class ReferencedAssemblies
    {
        private static readonly Lazy<IEnumerable<string>> LazyLocations;

        static ReferencedAssemblies()
        {
            LazyLocations = new Lazy<IEnumerable<string>>(() => HostingEnvironment.IsHosted
                ? GetAllReferencedAssembliesLocationFromBuildManager()
                : GetAllReferencedAssembliesFromDomain());
        }

        /// <summary>
        /// Gets the assembly locations of all the referenced assemblies, that
        /// are not dynamic, and have a non-null nor empty location.
        /// </summary>
        public static IEnumerable<string> Locations => LazyLocations.Value;

        public static Assembly GetNetStandardAssembly(List<Assembly> assemblies)
        {
            if (assemblies == null)
                assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToList();

            // for some reason, netstandard is also missing from BuildManager.ReferencedAssemblies and yet, is part of
            // the references that CSharpCompiler (above) receives - where is it coming from? cannot figure it out
            try
            {
                // so, resorting to an ugly trick
                // we should have System.Reflection.Metadata around, and it should reference netstandard
                var someAssembly = assemblies.First(x => x.FullName.StartsWith("System.Reflection.Metadata,"));
                var netStandardAssemblyName = someAssembly.GetReferencedAssemblies().First(x => x.FullName.StartsWith("netstandard,"));
                var netStandard = Assembly.Load(netStandardAssemblyName.FullName);
                return netStandard;
            }
            catch { /* never mind */ }

            return null;
        }

        public static Assembly GetNetStandardAssembly()
        {
            // in PreApplicationStartMethod we cannot get BuildManager.Referenced assemblies, do it differently
            try
            {
                var someAssembly = Assembly.Load("System.Reflection.Metadata");
                var netStandardAssemblyName = someAssembly.GetReferencedAssemblies().First(x => x.FullName.StartsWith("netstandard,"));
                var netStandard = Assembly.Load(netStandardAssemblyName.FullName);
                return netStandard;
            }
            catch { /* never mind */ }

            return null;
        }

        // hosted, get referenced assemblies from the BuildManager and filter
        private static IEnumerable<string> GetAllReferencedAssembliesLocationFromBuildManager()
        {
            var assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToList();

            assemblies.Add(typeof(ReferencedAssemblies).Assembly); // always include ourselves

            // see https://github.com/aspnet/RoslynCodeDomProvider/blob/master/src/Microsoft.CodeDom.Providers.DotNetCompilerPlatform/CSharpCompiler.cs:
            // mentions "Bug 913691: Explicitly add System.Runtime as a reference."
            // and explicitly adds System.Runtime to references before invoking csc.exe
            // so, doing the same here
            try
            {
                var systemRuntime = Assembly.Load("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                assemblies.Add(systemRuntime);
            }
            catch { /* never mind */ }

            // for some reason, netstandard is also missing from BuildManager.ReferencedAssemblies and yet, is part of
            // the references that CSharpCompiler (above) receives - where is it coming from? cannot figure it out
            var netStandard = GetNetStandardAssembly(assemblies);
            if (netStandard != null) assemblies.Add(netStandard);

            return assemblies
                .Where(x => !x.IsDynamic && !x.Location.IsNullOrWhiteSpace())
                .Select(x => x.Location)
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
