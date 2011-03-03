using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Web;
using System.IO;
using System.Linq;
using System.Xml;
using umbraco.IO;

namespace umbraco.BusinessLogic.Utils
{
    /// <summary>
    /// A utility class to find all classes of a certain type by reflection in the current bin folder
    /// of the web application. 
    /// </summary>
    public static class TypeFinder
    {
        // zb-00044 #29989 : refactor FindClassesMarkedWithAttribute

        /// <summary>
        /// Searches all loaded assemblies for classes marked with the attribute passed in.
        /// </summary>
        /// <returns>A list of found types</returns>
        public static IEnumerable<Type> FindClassesMarkedWithAttribute(Type attribute)
        {
            List<Type> types = new List<Type>();
            bool searchGAC = false;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // skip if the assembly is part of the GAC
                if (!searchGAC && assembly.GlobalAssemblyCache)
                    continue;

                types.AddRange(FindClassesMarkedWithAttribute(assembly, attribute));
            }

            // also add types from app_code, if any
            List<string> allowedExt = new List<string>();
            foreach (XmlNode x in UmbracoSettings.AppCodeFileExtensions)
                if (!String.IsNullOrEmpty(x.Value))
                    allowedExt.Add(x.Value);

            DirectoryInfo appCodeFolder = new DirectoryInfo(IOHelper.MapPath(IOHelper.ResolveUrl("~/App_code")));
            if (appCodeFolder.Exists && appCodeFolder.GetFilesByExtensions(true, allowedExt.ToArray()).Count() > 0)
            {
                types.AddRange(FindClassesMarkedWithAttribute(Assembly.Load("App_Code"), attribute));
            }

            return types.Distinct();
        }

        static IEnumerable<Type> FindClassesMarkedWithAttribute(Assembly assembly, Type attribute)
        {
            try
            {
                return assembly.GetTypes().Where(type => type.GetCustomAttributes(attribute, true).Length > 0);
            }
            catch (ReflectionTypeLoadException ex)
            {
                if (GlobalSettings.DebugMode)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Unable to load one or more of the types in assembly '{0}'. Exceptions were thrown:", assembly.FullName);
                    foreach (Exception e in ex.LoaderExceptions)
                        sb.AppendFormat("\n{0}: {1}", e.GetType().FullName, e.Message);
                    throw new Exception(sb.ToString());
                }
                else
                {
                    // return the types that were loaded, ignore those that could not be loaded
                    return ex.Types;
                }
            }
        }

        /// <summary>
        /// Searches all loaded assemblies for classes of the type passed in.
        /// </summary>
        /// <typeparam name="T">The type of object to search for</typeparam>
        /// <returns>A list of found types</returns>
        public static List<Type> FindClassesOfType<T>()
        {
            return FindClassesOfType<T>(true);
        }

        /// <summary>
        /// Searches all loaded assemblies for classes of the type passed in.
        /// </summary>
        /// <typeparam name="T">The type of object to search for</typeparam>
        /// <param name="useSeperateAppDomain">true if a seperate app domain should be used to query the types. This is safer as it is able to clear memory after loading the types.</param>
        /// <returns>A list of found types</returns>
        public static List<Type> FindClassesOfType<T>(bool useSeperateAppDomain)
        {
            return FindClassesOfType<T>(useSeperateAppDomain, true);
        }

        /// <summary>
        /// Searches all loaded assemblies for classes of the type passed in.
        /// </summary>
        /// <typeparam name="T">The type of object to search for</typeparam>
        /// <param name="useSeperateAppDomain">true if a seperate app domain should be used to query the types. This is safer as it is able to clear memory after loading the types.</param>
        /// <param name="onlyConcreteClasses">True to only return classes that can be constructed</param>
        /// <returns>A list of found types</returns>
        public static List<Type> FindClassesOfType<T>(bool useSeperateAppDomain, bool onlyConcreteClasses)
        {
            if (useSeperateAppDomain)
            {
                string binFolder = Path.Combine(IO.IOHelper.MapPath("/", false), "bin");
                string[] strTypes = TypeResolver.GetAssignablesFromType<T>(binFolder, "*.dll");




                List<Type> types = new List<Type>();

                foreach (string type in strTypes)
                    types.Add(Type.GetType(type));

                // also add types from app_code
                try
                {
                    // only search the App_Code folder if it's not empty
                    DirectoryInfo appCodeFolder = new DirectoryInfo(IOHelper.MapPath(IOHelper.ResolveUrl("~/App_code")));
                    if (appCodeFolder.GetFiles().Length > 0)
                    {
                        foreach (Type type in System.Reflection.Assembly.Load("App_Code").GetTypes())
                            types.Add(type);
                    }
                }
                catch { } // Empty catch - this just means that an App_Code assembly wasn't generated by the files in folder

                return types.FindAll(OnlyConcreteClasses(typeof(T), onlyConcreteClasses));
            }
            else
                return FindClassesOfType(typeof(T), onlyConcreteClasses);
        }

        /// <summary>
        /// Finds all classes with the specified type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A list of found classes</returns>
        private static List<Type> FindClassesOfType(Type type, bool onlyConcreteClasses)
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
                List<Type> foundTypes = listOfTypes.FindAll(OnlyConcreteClasses(type, onlyConcreteClasses));
                Type[] outputTypes = new Type[foundTypes.Count];
                foundTypes.CopyTo(outputTypes);
                classesOfType.AddRange(outputTypes);
            }

            return classesOfType;
        }

        private static Predicate<Type> OnlyConcreteClasses(Type type, bool onlyConcreteClasses)
        {
            return t => (type.IsAssignableFrom(t) && (onlyConcreteClasses ? (t.IsClass && !t.IsAbstract) : true));
        }

        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, bool searchSubdirs, params string[] extensions)
        {
            var allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
            IEnumerable<FileInfo> returnedFiles; 
            returnedFiles = dir.GetFiles().Where(f => allowedExtensions.Contains(f.Extension));
            if (searchSubdirs)
            {
                foreach(DirectoryInfo di in dir.GetDirectories())
                    returnedFiles.Concat(di.GetFilesByExtensions(true, extensions));
            }

            return returnedFiles;
        }
    }
}
