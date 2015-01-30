using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{

    /// <summary>
    /// A utility class to find all classes of a certain type by reflection in the current bin folder
    /// of the web application. 
    /// </summary>
    public static class TypeFinder
    {
        private static readonly HashSet<Assembly> LocalFilteredAssemblyCache = new HashSet<Assembly>();
        private static readonly ReaderWriterLockSlim LocalFilteredAssemblyCacheLocker = new ReaderWriterLockSlim();
        private static HashSet<Assembly> _allAssemblies = null;
        private static HashSet<Assembly> _binFolderAssemblies = null;
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        /// <summary>
        /// lazily load a reference to all assemblies and only local assemblies.
        /// This is a modified version of: http://www.dominicpettifer.co.uk/Blog/44/how-to-get-a-reference-to-all-assemblies-in-the--bin-folder
        /// </summary>
        /// <remarks>
        /// We do this because we cannot use AppDomain.Current.GetAssemblies() as this will return only assemblies that have been 
        /// loaded in the CLR, not all assemblies.
        /// See these threads:
        /// http://issues.umbraco.org/issue/U5-198
        /// http://stackoverflow.com/questions/3552223/asp-net-appdomain-currentdomain-getassemblies-assemblies-missing-after-app
        /// http://stackoverflow.com/questions/2477787/difference-between-appdomain-getassemblies-and-buildmanager-getreferencedassembl
        /// </remarks>
        internal static HashSet<Assembly> GetAllAssemblies()
        {
            using (var lck = new UpgradeableReadLock(Locker))
            {
                if (_allAssemblies == null)
                {

                    lck.UpgradeToWriteLock();

                    HashSet<Assembly> assemblies = null;
                    try
                    {
                        var isHosted = HttpContext.Current != null;

                        try
                        {
                            if (isHosted)
                            {
                                assemblies = new HashSet<Assembly>(BuildManager.GetReferencedAssemblies().Cast<Assembly>());
                            }
                        }
                        catch (InvalidOperationException e)
                        {
                            if (!(e.InnerException is SecurityException))
                                throw;
                        }


                        if (assemblies == null)
                        {
                            //NOTE: we cannot use AppDomain.CurrentDomain.GetAssemblies() because this only returns assemblies that have
                            // already been loaded in to the app domain, instead we will look directly into the bin folder and load each one.
                            var binFolder = IOHelper.GetRootDirectoryBinFolder();
                            var binAssemblyFiles = Directory.GetFiles(binFolder, "*.dll", SearchOption.TopDirectoryOnly).ToList();
                            //var binFolder = Assembly.GetExecutingAssembly().GetAssemblyFile().Directory;
                            //var binAssemblyFiles = Directory.GetFiles(binFolder.FullName, "*.dll", SearchOption.TopDirectoryOnly).ToList();
                            assemblies = new HashSet<Assembly>();
                            foreach (var a in binAssemblyFiles)
                            {
                                try
                                {
                                    var assName = AssemblyName.GetAssemblyName(a);
                                    var ass = Assembly.Load(assName);
                                    assemblies.Add(ass);
                                }
                                catch (Exception e)
                                {
                                    if (e is SecurityException || e is BadImageFormatException)
                                    {
                                        //swallow these exceptions
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                            }
                        }

                        //if for some reason they are still no assemblies, then use the AppDomain to load in already loaded assemblies.
                        if (!assemblies.Any())
                        {
                            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                            {
                                assemblies.Add(a);
                            }
                        }

                        //here we are trying to get the App_Code assembly
                        var fileExtensions = new[] { ".cs", ".vb" }; //only vb and cs files are supported
                        var appCodeFolder = new DirectoryInfo(IOHelper.MapPath(IOHelper.ResolveUrl("~/App_code")));
                        //check if the folder exists and if there are any files in it with the supported file extensions
                        if (appCodeFolder.Exists && (fileExtensions.Any(x => appCodeFolder.GetFiles("*" + x).Any())))
                        {
                            var appCodeAssembly = Assembly.Load("App_Code");
                            if (!assemblies.Contains(appCodeAssembly)) // BuildManager will find App_Code already
                                assemblies.Add(appCodeAssembly);
                        }

                        //now set the _allAssemblies
                        _allAssemblies = new HashSet<Assembly>(assemblies);

                    }
                    catch (InvalidOperationException e)
                    {
                        if (!(e.InnerException is SecurityException))
                            throw;

                        _binFolderAssemblies = _allAssemblies;
                    }
                }

                return _allAssemblies;
            }            
        }

        /// <summary>
        /// Returns only assemblies found in the bin folder that have been loaded into the app domain.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This will be used if we implement App_Plugins from Umbraco v5 but currently it is not used.
        /// </remarks>
        internal static HashSet<Assembly> GetBinAssemblies()
        {

            if (_binFolderAssemblies == null)
            {
                using (new WriteLock(Locker))
                {
                    var assemblies = GetAssembliesWithKnownExclusions().ToArray();
                    var binFolder = Assembly.GetExecutingAssembly().GetAssemblyFile().Directory;
                    var binAssemblyFiles = Directory.GetFiles(binFolder.FullName, "*.dll", SearchOption.TopDirectoryOnly).ToList();
                    var domainAssemblyNames = binAssemblyFiles.Select(AssemblyName.GetAssemblyName);
                    var safeDomainAssemblies = new HashSet<Assembly>();
                    var binFolderAssemblies = new HashSet<Assembly>();

                    foreach (var a in assemblies)
                    {
                        try
                        {
                            //do a test to see if its queryable in med trust
                            var assemblyFile = a.GetAssemblyFile();
                            safeDomainAssemblies.Add(a);
                        }
                        catch (SecurityException)
                        {
                            //we will just ignore this because this will fail 
                            //in medium trust for system assemblies, we get an exception but we just want to continue until we get to 
                            //an assembly that is ok.
                        }
                    }

                    foreach (var assemblyName in domainAssemblyNames)
                    {
                        try
                        {
                            var foundAssembly =
                                safeDomainAssemblies.FirstOrDefault(a => a.GetAssemblyFile() == assemblyName.GetAssemblyFile());
                            if (foundAssembly != null)
                            {
                                binFolderAssemblies.Add(foundAssembly);
                            }
                        }
                        catch (SecurityException)
                        {
                            //we will just ignore this because if we are trying to do a call to: 
                            // AssemblyName.ReferenceMatchesDefinition(a.GetName(), assemblyName)))
                            //in medium trust for system assemblies, we get an exception but we just want to continue until we get to 
                            //an assembly that is ok.
                        }
                    }

                    _binFolderAssemblies = new HashSet<Assembly>(binFolderAssemblies);
                }
            }
            return _binFolderAssemblies;
        }

        /// <summary>
        /// Return a list of found local Assemblies excluding the known assemblies we don't want to scan 
        /// and exluding the ones passed in and excluding the exclusion list filter, the results of this are
        /// cached for perforance reasons.
        /// </summary>
        /// <param name="excludeFromResults"></param>
        /// <returns></returns>
        internal static HashSet<Assembly> GetAssembliesWithKnownExclusions(
            IEnumerable<Assembly> excludeFromResults = null)
        {
            using (var lck = new UpgradeableReadLock(LocalFilteredAssemblyCacheLocker))
            {
                if (LocalFilteredAssemblyCache.Any()) return LocalFilteredAssemblyCache;

                lck.UpgradeToWriteLock();

                var assemblies = GetFilteredAssemblies(excludeFromResults, KnownAssemblyExclusionFilter);
                foreach (var a in assemblies)
                {
                    LocalFilteredAssemblyCache.Add(a);
                }

                return LocalFilteredAssemblyCache;
            }            
        }

        /// <summary>
        /// Return a distinct list of found local Assemblies and exluding the ones passed in and excluding the exclusion list filter
        /// </summary>
        /// <param name="excludeFromResults"></param>
        /// <param name="exclusionFilter"></param>
        /// <returns></returns>
        private static IEnumerable<Assembly> GetFilteredAssemblies(
            IEnumerable<Assembly> excludeFromResults = null,
            string[] exclusionFilter = null)
        {
            if (excludeFromResults == null)
                excludeFromResults = new HashSet<Assembly>();
            if (exclusionFilter == null)
                exclusionFilter = new string[] { };

            return GetAllAssemblies()
                .Where(x => !excludeFromResults.Contains(x)
                            && !x.GlobalAssemblyCache
                            && !exclusionFilter.Any(f => x.FullName.StartsWith(f)));
        }

        /// <summary>
        /// this is our assembly filter to filter out known types that def dont contain types we'd like to find or plugins
        /// </summary>
        /// <remarks>
        /// NOTE the comma vs period... comma delimits the name in an Assembly FullName property so if it ends with comma then its an exact name match
        /// </remarks>
        internal static readonly string[] KnownAssemblyExclusionFilter = new[]
                {
                    "mscorlib,",
					"System.",
                    "Antlr3.",
                    "Autofac.",
                    "Autofac,",
                    "Castle.",
                    "ClientDependency.",
                    "DataAnnotationsExtensions.",
                    "DataAnnotationsExtensions,",
                    "Dynamic,",
                    "HtmlDiff,",
                    "Iesi.Collections,",
                    "log4net,",
                    "Microsoft.",
                    "Newtonsoft.",
                    "NHibernate.",
                    "NHibernate,",
                    "NuGet.",
                    "RouteDebugger,",
                    "SqlCE4Umbraco,",
                    "umbraco.datalayer,",
                    "umbraco.interfaces,",										
					//"umbraco.providers,",
					//"Umbraco.Web.UI,",
                    "umbraco.webservices",
                    "Lucene.",
                    "Examine,",
                    "Examine.",
                    "ServiceStack.",
                    "MySql.",
                    "HtmlAgilityPack.",
                    "TidyNet.",
                    "ICSharpCode.",
                    "CookComputing.",
                    "AutoMapper,",
                    "AutoMapper.",
                    "AzureDirectory,",
                    "itextsharp,",            
                    "UrlRewritingNet.",
                    "HtmlAgilityPack,",                 
                    "MiniProfiler,",
                    "Moq,",
                    "nunit.framework,",
                    "TidyNet,",
                    "WebDriver,"
                };

        /// <summary>
        /// Finds any classes derived from the type T that contain the attribute TAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> FindClassesOfTypeWithAttribute<T, TAttribute>()
            where TAttribute : Attribute
        {
            return FindClassesOfTypeWithAttribute<T, TAttribute>(GetAssembliesWithKnownExclusions(), true);
        }

        /// <summary>
        /// Finds any classes derived from the type T that contain the attribute TAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IEnumerable<Type> FindClassesOfTypeWithAttribute<T, TAttribute>(IEnumerable<Assembly> assemblies)
            where TAttribute : Attribute
        {
            return FindClassesOfTypeWithAttribute<T, TAttribute>(assemblies, true);
        }

        /// <summary>
        /// Finds any classes derived from the type T that contain the attribute TAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="assemblies"></param>
        /// <param name="onlyConcreteClasses"></param>
        /// <returns></returns>
        public static IEnumerable<Type> FindClassesOfTypeWithAttribute<T, TAttribute>(
            IEnumerable<Assembly> assemblies,
            bool onlyConcreteClasses)
            where TAttribute : Attribute
        {
            return FindClassesOfTypeWithAttribute<TAttribute>(typeof(T), assemblies, onlyConcreteClasses);
        }

        /// <summary>
        /// Finds any classes derived from the assignTypeFrom Type that contain the attribute TAttribute
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="assignTypeFrom"></param>
        /// <param name="assemblies"></param>
        /// <param name="onlyConcreteClasses"></param>
        /// <returns></returns>
        public static IEnumerable<Type> FindClassesOfTypeWithAttribute<TAttribute>(
            Type assignTypeFrom,
            IEnumerable<Assembly> assemblies,
            bool onlyConcreteClasses)
            where TAttribute : Attribute
        {
            if (assemblies == null) throw new ArgumentNullException("assemblies");

            return GetClasses(assignTypeFrom, assemblies, onlyConcreteClasses,
                //the additional filter will ensure that any found types also have the attribute applied.
                t => t.GetCustomAttributes<TAttribute>(false).Any());
        }

        /// <summary>
        /// Searches all filtered local assemblies specified for classes of the type passed in.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> FindClassesOfType<T>()
        {
            return FindClassesOfType<T>(GetAssembliesWithKnownExclusions(), true);
        }

        /// <summary>
        /// Returns all types found of in the assemblies specified of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblies"></param>
        /// <param name="onlyConcreteClasses"></param>
        /// <returns></returns>
        public static IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses)
        {
            if (assemblies == null) throw new ArgumentNullException("assemblies");

            return GetClasses(typeof(T), assemblies, onlyConcreteClasses);
        }

        /// <summary>
        /// Returns all types found of in the assemblies specified of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies)
        {
            return FindClassesOfType<T>(assemblies, true);
        }

        /// <summary>
        /// Finds the classes with attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="onlyConcreteClasses">if set to <c>true</c> only concrete classes.</param>
        /// <returns></returns>
        public static IEnumerable<Type> FindClassesWithAttribute<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses)
            where T : Attribute
        {
            return FindClassesWithAttribute(typeof(T), assemblies, onlyConcreteClasses);
        }

        /// <summary>
        /// Finds any classes with the attribute.
        /// </summary>
        /// <param name="attributeType">The attribute type </param>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="onlyConcreteClasses">if set to <c>true</c> only concrete classes.</param>
        /// <returns></returns>
        public static IEnumerable<Type> FindClassesWithAttribute(
            Type attributeType,
            IEnumerable<Assembly> assemblies,
            bool onlyConcreteClasses)
        {
            if (assemblies == null) throw new ArgumentNullException("assemblies");

            if (TypeHelper.IsTypeAssignableFrom<Attribute>(attributeType) == false)
                throw new ArgumentException("The type specified: " + attributeType + " is not an Attribute type");

            var foundAttributedTypes = new HashSet<Type>();

            var assemblyList = assemblies.ToArray();

            //find all assembly references that are referencing the attribute type's assembly since we 
            //should only be scanning those assemblies because any other assembly will definitely not
            //contain a class that has this attribute.
            var referencedAssemblies = TypeHelper.GetReferencedAssemblies(attributeType, assemblyList);

            //get a list of non-referenced assemblies (we'll use this when we recurse below)
            var otherAssemblies = assemblyList.Where(x => referencedAssemblies.Contains(x) == false).ToArray();

            //loop through the referenced assemblies
            foreach (var a in referencedAssemblies)
            {
                //get all types in this assembly
                var allTypes = GetTypesWithFormattedException(a)
                    .ToArray();

                var attributedTypes = new Type[] {};
                try
                {
                    //now filter the types based on the onlyConcreteClasses flag, not interfaces, not static classes but have
                    //the specified attribute
                    attributedTypes = allTypes
                        .Where(t => (TypeHelper.IsNonStaticClass(t)
                                     && (onlyConcreteClasses == false || t.IsAbstract == false))
                            //the type must have this attribute
                                     && t.GetCustomAttributes(attributeType, false).Any())
                        .ToArray();
                }
                catch (TypeLoadException ex)
                {
                    LogHelper.Error(typeof(TypeFinder), string.Format("Could not query types on {0} assembly, this is most likely due to this assembly not being compatible with the current Umbraco version", a), ex);
                    continue;
                }

                //add the types to our list to return
                foreach (var t in attributedTypes)
                {
                    foundAttributedTypes.Add(t);
                }

                //get all attributes of the type being searched for
                var allAttributeTypes = allTypes.Where(attributeType.IsAssignableFrom);

                //now we need to include types that may be inheriting from sub classes of the attribute type being searched for
                //so we will search in assemblies that reference those types too.
                foreach (var subTypesInAssembly in allAttributeTypes.GroupBy(x => x.Assembly)){

                    //So that we are not scanning too much, we need to group the sub types:    
                    // * if there is more than 1 sub type in the same assembly then we should only search on the 'lowest base' type.
                    // * We should also not search for sub types if the type is sealed since you cannot inherit from a sealed class
                    // * We should not search for sub types if the type is static since you cannot inherit from them.
                    var subTypeList = subTypesInAssembly
                        .Where(t => t.IsSealed == false && TypeHelper.IsStaticClass(t) == false)
                        .ToArray();

                    var baseClassAttempt = TypeHelper.GetLowestBaseType(subTypeList);

                    //if there's a base class amongst the types then we'll only search for that type.
                    //otherwise we'll have to search for all of them.
                    var subTypesToSearch = new HashSet<Type>();
                    if (baseClassAttempt.Success)
                    {
                        subTypesToSearch.Add(baseClassAttempt.Result);
                    }
                    else
                    {
                        foreach (var t in subTypeList)
                        {
                            subTypesToSearch.Add(t);
                        }
                    }

                    foreach (var typeToSearch in subTypesToSearch)
                    {
                        //recursively find the types inheriting from this sub type in the other non-scanned assemblies.
                        var foundTypes = FindClassesWithAttribute(typeToSearch, otherAssemblies, onlyConcreteClasses);

                        foreach (var f in foundTypes)
                        {
                            foundAttributedTypes.Add(f);
                        }
                    }

                }
            }

            return foundAttributedTypes;
        }


        /// <summary>
        /// Finds the classes with attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns></returns>
        public static IEnumerable<Type> FindClassesWithAttribute<T>(IEnumerable<Assembly> assemblies)
            where T : Attribute
        {
            return FindClassesWithAttribute<T>(assemblies, true);
        }

        /// <summary>
        /// Finds the classes with attribute in filtered local assemblies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> FindClassesWithAttribute<T>()
            where T : Attribute
        {
            return FindClassesWithAttribute<T>(GetAssembliesWithKnownExclusions());
        }


        #region Private methods

        /// <summary>
        /// Finds types that are assignable from the assignTypeFrom parameter and will scan for these types in the assembly
        /// list passed in, however we will only scan assemblies that have a reference to the assignTypeFrom Type or any type 
        /// deriving from the base type.
        /// </summary>
        /// <param name="assignTypeFrom"></param>
        /// <param name="assemblies"></param>
        /// <param name="onlyConcreteClasses"></param>
        /// <param name="additionalFilter">An additional filter to apply for what types will actually be included in the return value</param>
        /// <returns></returns>
        private static IEnumerable<Type> GetClasses(
            Type assignTypeFrom,
            IEnumerable<Assembly> assemblies,
            bool onlyConcreteClasses,
            Func<Type, bool> additionalFilter = null)
        {
            //the default filter will always return true.
            if (additionalFilter == null)
            {
                additionalFilter = type => true;
            }

            var foundAssignableTypes = new HashSet<Type>();

            var assemblyList = assemblies.ToArray();

            //find all assembly references that are referencing the current type's assembly since we 
            //should only be scanning those assemblies because any other assembly will definitely not
            //contain sub type's of the one we're currently looking for
            var referencedAssemblies = TypeHelper.GetReferencedAssemblies(assignTypeFrom, assemblyList);

            //get a list of non-referenced assemblies (we'll use this when we recurse below)
            var otherAssemblies = assemblyList.Where(x => referencedAssemblies.Contains(x) == false).ToArray();

            //loop through the referenced assemblies
            foreach (var a in referencedAssemblies)
            {
                //get all types in the assembly that are sub types of the current type
                var allSubTypes = GetTypesWithFormattedException(a)
                    .Where(assignTypeFrom.IsAssignableFrom)
                    .ToArray();

                var filteredTypes = new Type[] { };
                try
                {
                    //now filter the types based on the onlyConcreteClasses flag, not interfaces, not static classes
                    filteredTypes = allSubTypes
                        .Where(t => (TypeHelper.IsNonStaticClass(t)
                            //Do not include nested private classes - since we are in full trust now this will find those too!
                                     && t.IsNestedPrivate == false
                                     && (onlyConcreteClasses == false || t.IsAbstract == false)
                            //Do not include classes that are flagged to hide from the type finder
                                     && t.GetCustomAttribute<HideFromTypeFinderAttribute>() == null
                                     && additionalFilter(t)))
                        .ToArray();
                }
                catch (TypeLoadException ex)
                {
                    LogHelper.Error(typeof(TypeFinder), string.Format("Could not query types on {0} assembly, this is most likely due to this assembly not being compatible with the current Umbraco version", a), ex);
                    continue; 
                }

                //add the types to our list to return
                foreach (var t in filteredTypes)
                {
                    foundAssignableTypes.Add(t);
                }
                
                //now we need to include types that may be inheriting from sub classes of the type being searched for
                //so we will search in assemblies that reference those types too.
                foreach (var subTypesInAssembly in allSubTypes.GroupBy(x => x.Assembly))
                {

                    //So that we are not scanning too much, we need to group the sub types:    
                    // * if there is more than 1 sub type in the same assembly then we should only search on the 'lowest base' type.
                    // * We should also not search for sub types if the type is sealed since you cannot inherit from a sealed class
                    // * We should not search for sub types if the type is static since you cannot inherit from them.
                    var subTypeList = subTypesInAssembly
                        .Where(t => t.IsSealed == false && TypeHelper.IsStaticClass(t) == false)
                        .ToArray();

                    var baseClassAttempt = TypeHelper.GetLowestBaseType(subTypeList);

                    //if there's a base class amongst the types then we'll only search for that type.
                    //otherwise we'll have to search for all of them.
                    var subTypesToSearch = new HashSet<Type>();
                    if (baseClassAttempt.Success)
                    {
                        subTypesToSearch.Add(baseClassAttempt.Result);
                    }
                    else
                    {
                        foreach (var t in subTypeList)
                        {
                            subTypesToSearch.Add(t);
                        }
                    }

                    foreach (var typeToSearch in subTypesToSearch)
                    {
                        //recursively find the types inheriting from this sub type in the other non-scanned assemblies.
                        var foundTypes = GetClasses(typeToSearch, otherAssemblies, onlyConcreteClasses, additionalFilter);

                        foreach (var f in foundTypes)
                        {
                            foundAssignableTypes.Add(f);
                        }
                    }

                }

            }
            return foundAssignableTypes;
        }

        private static IEnumerable<Type> GetTypesWithFormattedException(Assembly a)
        {
            //if the assembly is dynamic, do not try to scan it
            if (a.IsDynamic)
                return Enumerable.Empty<Type>();

            try
            {
                //we need to detect if an assembly is partially trusted, if so we cannot go interrogating all of it's types
                //only its exported types, otherwise we'll get exceptions.
                if (a.GetCustomAttribute<AllowPartiallyTrustedCallersAttribute>() == null)
                {
                    return a.GetTypes();
                }
                else
                {
                    return a.GetExportedTypes();
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Could not load types from assembly " + a.FullName + ", errors:");
                foreach (var loaderException in ex.LoaderExceptions.WhereNotNull())
                {
                    sb.AppendLine("Exception: " + loaderException);
                }
                throw new ReflectionTypeLoadException(ex.Types, ex.LoaderExceptions, sb.ToString());
            }
        }

        #endregion

        public static Type GetTypeByName(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.GetType(typeName))
                .FirstOrDefault(x => x != null);
        }

    }
}
