using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Web;
using System.IO;

namespace umbraco.BusinessLogic.Utils
{
    /// <summary>
    /// A utility class to find all classes of a certain type by reflection in the current bin folder
    /// of the web application. 
    /// </summary>
	public static class TypeFinder
	{

		/// <summary>
        /// Searches all loaded assemblies for classes of the type passed in.
		/// </summary>
		/// <typeparam name="T">The type of object to search for</typeparam>
		/// <param name="useSeperateAppDomain">true if a seperate app domain should be used to query the types. This is safer as it is able to clear memory after loading the types.</param>
		/// <returns>A list of found types</returns>
		public static List<Type> FindClassesOfType<T>(bool useSeperateAppDomain)
		{
            if (useSeperateAppDomain)
            {
                string binFolder = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "bin");
                string[] strTypes = TypeResolver.GetAssignablesFromType<T>(binFolder, "*.dll");

                List<Type> types = new List<Type>();

                foreach (string type in strTypes)
                    types.Add(Type.GetType(type));

                return types;
            }
            else
                return FindClassesOfType(typeof(T));
        }

        /// <summary>
        /// Finds all classes with the specified type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A list of found classes</returns>
        private static List<Type> FindClassesOfType(Type type)
        {
            bool searchGAC = false;

            List<Type> classesOfType = new List<Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                //don't check any types if the assembly is part of the GAC
                if (!searchGAC && assembly.GlobalAssemblyCache)
                    continue;

                Type[] publicTypes;
                //need try catch block as some assemblies do not support this (i.e.RefEmit_InMemoryManifestModule)
                try
                {
                    publicTypes = assembly.GetExportedTypes();
                }
                catch { continue; }

                List<Type> listOfTypes = new List<Type>();
                listOfTypes.AddRange(publicTypes);
                List<Type> foundTypes = listOfTypes.FindAll(
                    delegate(Type t) { return (type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract); }
                );
                Type[] outputTypes = new Type[foundTypes.Count];
                foundTypes.CopyTo(outputTypes);
                classesOfType.AddRange(outputTypes);
            }

            return classesOfType;
        }

	}
}
