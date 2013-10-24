using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Packaging
{
    internal class PackageBinaryInspector
    {

        public static IEnumerable<string> BinariesContainInstanceOf<T>(string dllPath)
        {
            var result = new List<string>();

            if (Directory.Exists(dllPath) == false)
            {
                throw new DirectoryNotFoundException("Could not find directory " + dllPath);
            }
            var files = Directory.GetFiles(dllPath, "*.dll");

            //we need this handler to resolve assembly dependencies below
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (s, e) =>
            {
                var ass = Assembly.ReflectionOnlyLoad(e.Name);
                if (ass == null)
                {
                    throw new TypeLoadException("Could not load assembly " + e.Name);
                }
                return ass;
            };

            //First load each dll file into the context
            foreach (var f in files)
            {
                if (File.Exists(f) == false)
                {
                    throw new FileNotFoundException("Could not find file " + f);
                }                
                Assembly.ReflectionOnlyLoadFrom(f);
            }

            var toIgnore = new List<string>();

            //Then load each referenced assembly into the context
            foreach (var f in files)
            {
                var reflectedAssembly = Assembly.ReflectionOnlyLoadFrom(f);
                foreach (var assemblyName in reflectedAssembly.GetReferencedAssemblies())
                {
                    try
                    {
                        Assembly.ReflectionOnlyLoad(assemblyName.FullName);
                    }
                    catch (Exception)
                    {
                        //if an exception occurs it means that a referenced assembly could not be found  - unless something else strange is going on.
                        //we'll log an error for this to return in our report
                        result.Add("This package references an assembly that was not found (" + assemblyName.FullName + "), this package may have problems running");
                        toIgnore.Add(f);
                    }
                }
            }

            //now that we have all referenced types into the context we can look up stuff
            foreach (var f in files.Except(toIgnore))
            {
                //now we need to see if they contain any type 'T'
                var reflectedAssembly = Assembly.ReflectionOnlyLoadFrom(f);
                var found = reflectedAssembly.GetExportedTypes().Where(TypeHelper.IsTypeAssignableFrom<T>).ToArray();
                result.AddRange(found.Select(x => x.FullName));
            }

            return result;
        }

    }
}
