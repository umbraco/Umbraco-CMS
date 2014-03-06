using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Umbraco.Web.Standalone
{
    internal static class PowershellAssemblyResolver
    {
        private static readonly Dictionary<string, string> Assemblies;
        private static readonly object Locko = new object();

        static PowershellAssemblyResolver()
        {
            var comparer = StringComparer.CurrentCultureIgnoreCase;
            Assemblies = new Dictionary<string,string>(comparer);
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;
        }

        public static void AddAssemblyLocation(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Arg is null or empty.", "path");

            lock (Locko)
            {
                var name = Path.GetFileNameWithoutExtension(path);
                Assemblies.Add(name, path);
            }
        }

        private static Assembly ResolveHandler(object sender, ResolveEventArgs args) 
        {
            var assemblyName = new AssemblyName(args.Name);
            string assemblyFile;
            return Assemblies.TryGetValue(assemblyName.Name, out assemblyFile)
                ? Assembly.LoadFrom(assemblyFile)
                : null;
        }
    }
}
