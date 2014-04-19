using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Umbraco.Core
{
	internal static class AssemblyExtensions
	{
		/// <summary>
		/// Returns the file used to load the assembly
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		public static FileInfo GetAssemblyFile(this Assembly assembly)
		{
			var codeBase = assembly.CodeBase;
			var uri = new Uri(codeBase);
			var path = uri.LocalPath;
			return new FileInfo(path);
		}

        /// <summary>
        /// Returns true if the assembly is the App_Code assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static bool IsAppCodeAssembly(this Assembly assembly)
        {
            if (assembly.FullName.StartsWith("App_Code"))
            {
                try
                {
                    Assembly.Load("App_Code");
                    return true;
                }
                catch (FileNotFoundException)
                {
                    //this will occur if it cannot load the assembly
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the assembly is the compiled global asax.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
	    public static bool IsGlobalAsaxAssembly(this Assembly assembly)
	    {
            //only way I can figure out how to test is by the name
            return assembly.FullName.StartsWith("App_global.asax");
	    }

	    /// <summary>
		///  Returns the file used to load the assembly
		/// </summary>
		/// <param name="assemblyName"></param>
		/// <returns></returns>
		public static FileInfo GetAssemblyFile(this AssemblyName assemblyName)
		{
			var codeBase = assemblyName.CodeBase;
			var uri = new Uri(codeBase);
			var path = uri.LocalPath;
			return new FileInfo(path);
		}

        /// <summary>
        /// Gets the <see cref="AssemblyName"/> objects for all the assemblies recursively referenced by a specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The <see cref="AssemblyName"/> objects for all the assemblies recursively referenced by the specified assembly.</returns>
        public static IEnumerable<AssemblyName> GetDeepReferencedAssemblies(this Assembly assembly)
        {
            var allAssemblies = TypeFinder.GetAllAssemblies();
            var visiting = new Stack<Assembly>();
            var visited = new HashSet<Assembly>();

            visiting.Push(assembly);
            visited.Add(assembly);
            while (visiting.Count > 0)
            {
                var visAsm = visiting.Pop();
                foreach (var refAsm in visAsm.GetReferencedAssemblies()
                    .Select(refAsmName => allAssemblies.SingleOrDefault(x => string.Equals(x.GetName().Name, refAsmName.Name, StringComparison.Ordinal)))
                    .Where(x => x != null && visited.Contains(x) == false))
                {
                    yield return refAsm.GetName();
                    visiting.Push(refAsm);
                    visited.Add(refAsm);
                }
            }
        }
	}
}