using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Web.Compilation;
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
        private static volatile HashSet<Assembly> _localFilteredAssemblyCache;
        private static readonly object LocalFilteredAssemblyCacheLocker = new object();
        private static readonly List<string> NotifiedLoadExceptionAssemblies = new List<string>();
        private static string[] _assembliesAcceptingLoadExceptions;

        private static string[] AssembliesAcceptingLoadExceptions
        {
            get
            {
                if (_assembliesAcceptingLoadExceptions != null)
                    return _assembliesAcceptingLoadExceptions;

                var s = ConfigurationManager.AppSettings["Umbraco.AssembliesAcceptingLoadExceptions"];
                return _assembliesAcceptingLoadExceptions = string.IsNullOrWhiteSpace(s)
                    ? new string[0]
                    : s.Split(',').Select(x => x.Trim()).ToArray();
            }
        }

        private static bool AcceptsLoadExceptions(Assembly a)
        {
            if (AssembliesAcceptingLoadExceptions.Length == 0)
                return false;
            if (AssembliesAcceptingLoadExceptions.Length == 1 && AssembliesAcceptingLoadExceptions[0] == "*")
                return true;
            var name = a.GetName().Name; // simple name of the assembly
            return AssembliesAcceptingLoadExceptions.Any(pattern =>
            {
                if (pattern.Length > name.Length) return false; // pattern longer than name
                if (pattern.Length == name.Length) return pattern.InvariantEquals(name); // same length, must be identical
                if (pattern[pattern.Length] != '.') return false; // pattern is shorter than name, must end with dot
                return name.StartsWith(pattern); // and name must start with pattern
            });
        }

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
            return AllAssemblies.Value;
        }

        //Lazy access to the all assemblies list
        private static readonly Lazy<HashSet<Assembly>> AllAssemblies = new Lazy<HashSet<Assembly>>(() =>
        {
            HashSet<Assembly> assemblies = null;
            try
            {
                var isHosted = IOHelper.IsHosted;

                try
                {
                    if (isHosted)
                    {
                        assemblies = new HashSet<Assembly>(BuildManager.GetReferencedAssemblies().Cast<Assembly>());
                    }
                }
                catch (InvalidOperationException e)
                {
                    if ((e.InnerException is SecurityException) == false)
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
                if (assemblies.Any() == false)
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
                if (appCodeFolder.Exists && fileExtensions.Any(x => appCodeFolder.GetFiles("*" + x).Any()))
                {
                    try
                    {
                        var appCodeAssembly = Assembly.Load("App_Code");
                        if (assemblies.Contains(appCodeAssembly) == false) // BuildManager will find App_Code already
                            assemblies.Add(appCodeAssembly);
                    }
                    catch (FileNotFoundException ex)
                    {
                        //this will occur if it cannot load the assembly
                        LogHelper.Error(typeof(TypeFinder), "Could not load assembly App_Code", ex);
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                if (e.InnerException is SecurityException == false)
                    throw;
            }

            return assemblies;
        });

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
            lock (LocalFilteredAssemblyCacheLocker)
            {
                if (_localFilteredAssemblyCache != null)
                    return _localFilteredAssemblyCache;

                var assemblies = GetFilteredAssemblies(excludeFromResults, KnownAssemblyExclusionFilter);
                _localFilteredAssemblyCache = new HashSet<Assembly>(assemblies);
                return _localFilteredAssemblyCache;
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
                .Where(x => excludeFromResults.Contains(x) == false
                            && x.GlobalAssemblyCache == false
                            && exclusionFilter.Any(f => x.FullName.StartsWith(f)) == false);
        }

        /// <summary>
        /// this is our assembly filter to filter out known types that def dont contain types we'd like to find or plugins
        /// </summary>
        /// <remarks>
        /// NOTE the comma vs period... comma delimits the name in an Assembly FullName property so if it ends with comma then its an exact name match
        /// NOTE this means that "foo." will NOT exclude "foo.dll" but only "foo.*.dll"
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

            return GetClassesWithBaseType(assignTypeFrom, assemblies, onlyConcreteClasses,
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

            return GetClassesWithBaseType(typeof(T), assemblies, onlyConcreteClasses);
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
            return GetClassesWithAttribute(attributeType, assemblies, onlyConcreteClasses);
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

        private static IEnumerable<Type> GetClassesWithAttribute(
            Type attributeType,
            IEnumerable<Assembly> assemblies,
            bool onlyConcreteClasses)
        {
            if (typeof(Attribute).IsAssignableFrom(attributeType) == false)
                throw new ArgumentException("Type " + attributeType + " is not an Attribute type.");

            var candidateAssemblies = new HashSet<Assembly>(assemblies);
            var attributeAssemblyIsCandidate = candidateAssemblies.Contains(attributeType.Assembly);
            candidateAssemblies.Remove(attributeType.Assembly);
            var types = new List<Type>();

            var stack = new Stack<Assembly>();
            stack.Push(attributeType.Assembly);

            while (stack.Count > 0)
            {
                var assembly = stack.Pop();

                Type[] assemblyTypes = null;
                if (assembly != attributeType.Assembly || attributeAssemblyIsCandidate)
                {
                    // get all assembly types that can be assigned to baseType
                    try
                    {
                        assemblyTypes = GetTypesWithFormattedException(assembly)
                            .ToArray(); // in try block
                    }
                    catch (TypeLoadException ex)
                    {
                        LogHelper.Error(typeof(TypeFinder), string.Format("Could not query types on {0} assembly, this is most likely due to this assembly not being compatible with the current Umbraco version", assembly), ex);
                        continue;
                    }

                    types.AddRange(assemblyTypes.Where(x =>
                        x.IsClass // only classes
                        && (x.IsAbstract == false || x.IsSealed == false) // ie non-static, static is abstract and sealed
                        && x.IsNestedPrivate == false // exclude nested private
                        && (onlyConcreteClasses == false || x.IsAbstract == false) // exclude abstract
                        && x.GetCustomAttribute<HideFromTypeFinderAttribute>() == null // exclude hidden
                        && x.GetCustomAttributes(attributeType, false).Any())); // marked with the attribute
                }

                if (assembly != attributeType.Assembly && assemblyTypes.Where(attributeType.IsAssignableFrom).Any() == false)
                    continue;

                foreach (var referencing in TypeHelper.GetReferencingAssemblies(assembly, candidateAssemblies))
                {
                    candidateAssemblies.Remove(referencing);
                    stack.Push(referencing);
                }
            }

            return types;
        }

        /// <summary>
        /// Finds types that are assignable from the assignTypeFrom parameter and will scan for these types in the assembly
        /// list passed in, however we will only scan assemblies that have a reference to the assignTypeFrom Type or any type
        /// deriving from the base type.
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="assemblies"></param>
        /// <param name="onlyConcreteClasses"></param>
        /// <param name="additionalFilter">An additional filter to apply for what types will actually be included in the return value</param>
        /// <returns></returns>
        private static IEnumerable<Type> GetClassesWithBaseType(
            Type baseType,
            IEnumerable<Assembly> assemblies,
            bool onlyConcreteClasses,
            Func<Type, bool> additionalFilter = null)
        {
            var candidateAssemblies = new HashSet<Assembly>(assemblies);
            var baseTypeAssemblyIsCandidate = candidateAssemblies.Contains(baseType.Assembly);
            candidateAssemblies.Remove(baseType.Assembly);
            var types = new List<Type>();

            var stack = new Stack<Assembly>();
            stack.Push(baseType.Assembly);

            while (stack.Count > 0)
            {
                var assembly = stack.Pop();

                // get all assembly types that can be assigned to baseType
                Type[] assemblyTypes = null;
                if (assembly != baseType.Assembly || baseTypeAssemblyIsCandidate)
                {
                    try
                    {
                        assemblyTypes = GetTypesWithFormattedException(assembly)
                            .Where(baseType.IsAssignableFrom)
                            .ToArray(); // in try block
                    }
                    catch (TypeLoadException ex)
                    {
                        LogHelper.Error(typeof(TypeFinder), string.Format("Could not query types on {0} assembly, this is most likely due to this assembly not being compatible with the current Umbraco version", assembly), ex);
                        continue;
                    }

                    types.AddRange(assemblyTypes.Where(x =>
                        x.IsClass // only classes
                        && (x.IsAbstract == false || x.IsSealed == false) // ie non-static, static is abstract and sealed
                        && x.IsNestedPrivate == false // exclude nested private
                        && (onlyConcreteClasses == false || x.IsAbstract == false) // exclude abstract
                        && x.GetCustomAttribute<HideFromTypeFinderAttribute>() == null // exclude hidden
                        && (additionalFilter == null || additionalFilter(x)))); // filter
                }

                if (assembly != baseType.Assembly && assemblyTypes.All(x => x.IsSealed))
                    continue;

                foreach (var referencing in TypeHelper.GetReferencingAssemblies(assembly, candidateAssemblies))
                {
                    candidateAssemblies.Remove(referencing);
                    stack.Push(referencing);
                }
            }

            return types;
        }

        internal static IEnumerable<Type> GetTypesWithFormattedException(Assembly a)
        {
            //if the assembly is dynamic, do not try to scan it
            if (a.IsDynamic)
                return Enumerable.Empty<Type>();

            var getAll = a.GetCustomAttribute<AllowPartiallyTrustedCallersAttribute>() == null;

            try
            {
                //we need to detect if an assembly is partially trusted, if so we cannot go interrogating all of it's types
                //only its exported types, otherwise we'll get exceptions.
                return getAll ? a.GetTypes() : a.GetExportedTypes();
            }
            catch (TypeLoadException ex) // GetExportedTypes *can* throw TypeLoadException!
            {
                var sb = new StringBuilder();
                AppendCouldNotLoad(sb, a, getAll);
                AppendLoaderException(sb, ex);

                // rethrow as ReflectionTypeLoadException (for consistency) with new message
                throw new ReflectionTypeLoadException(new Type[0], new Exception[] { ex }, sb.ToString());
            }
            catch (ReflectionTypeLoadException rex) // GetTypes throws ReflectionTypeLoadException
            {
                var sb = new StringBuilder();
                AppendCouldNotLoad(sb, a, getAll);
                foreach (var loaderException in rex.LoaderExceptions.WhereNotNull())
                    AppendLoaderException(sb, loaderException);

                var ex = new ReflectionTypeLoadException(rex.Types, rex.LoaderExceptions, sb.ToString());

                // rethrow with new message, unless accepted
                if (AcceptsLoadExceptions(a) == false) throw ex;

                // log a warning, and return what we can
                lock (NotifiedLoadExceptionAssemblies)
                {
                    if (NotifiedLoadExceptionAssemblies.Contains(a.FullName) == false)
                    {
                        NotifiedLoadExceptionAssemblies.Add(a.FullName);
                        LogHelper.WarnWithException(typeof(TypeFinder), "Could not load all types from " + a.GetName().Name + ".", ex);
                    }
                }
                return rex.Types.WhereNotNull().ToArray();
            }
        }

        private static void AppendCouldNotLoad(StringBuilder sb, Assembly a, bool getAll)
        {
            sb.Append("Could not load ");
            sb.Append(getAll ? "all" : "exported");
            sb.Append(" types from \"");
            sb.Append(a.FullName);
            sb.AppendLine("\" due to LoaderExceptions, skipping:");
        }

        private static void AppendLoaderException(StringBuilder sb, Exception loaderException)
        {
            sb.Append(". ");
            sb.Append(loaderException.GetType().FullName);

            var tloadex = loaderException as TypeLoadException;
            if (tloadex != null)
            {
                sb.Append(" on ");
                sb.Append(tloadex.TypeName);
            }

            sb.Append(": ");
            sb.Append(loaderException.Message);
            sb.AppendLine();
        }

        #endregion

        public static Type GetTypeByName(string typeName)
        {
            var type = BuildManager.GetType(typeName, false);
            if (type != null) return type;

            //TODO: This isn't very elegant, and will have issues since the AppDomain.CurrentDomain
            // doesn't actualy load in all assemblies, only the types that have been referenced so far.
            // However, in a web context, the BuildManager will have executed which will force all assemblies
            // to be loaded so it's fine for now.

            //now try fall back procedures.
            type = Type.GetType(typeName);
            if (type != null) return type;
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.GetType(typeName))
                .FirstOrDefault(x => x != null);
        }
    }
}
