using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using NUnit.Framework;
using SqlCE4Umbraco;
using umbraco;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.DataLayer;
using umbraco.editorControls.tags;
using umbraco.interfaces;
using umbraco.MacroEngines;
using umbraco.uicontrols;
using Umbraco.Web.BaseRest;

namespace Umbraco.Tests.Plugins
{
    
    /// <summary>
	/// Tests for typefinder
	/// </summary>
	[TestFixture]	
	public class TypeFinderTests
	{
		/// <summary>
		/// List of assemblies to scan
		/// </summary>
		private Assembly[] _assemblies;

		[SetUp]
		public void Initialize()
		{
		    _assemblies = new[]
			    {
			        this.GetType().Assembly, 
			        typeof(ApplicationStartupHandler).Assembly,
			        typeof(SqlCEHelper).Assembly,
			        typeof(CMSNode).Assembly,
			        typeof(System.Guid).Assembly,
			        typeof(NUnit.Framework.Assert).Assembly,
			        typeof(Microsoft.CSharp.CSharpCodeProvider).Assembly,
			        typeof(System.Xml.NameTable).Assembly,
			        typeof(System.Configuration.GenericEnumConverter).Assembly,
			        typeof(System.Web.SiteMap).Assembly,
			        typeof(TabPage).Assembly,
			        typeof(System.Web.Mvc.ActionResult).Assembly,
			        typeof(TypeFinder).Assembly,
			        typeof(ISqlHelper).Assembly,
			        typeof(ICultureDictionary).Assembly,
                    typeof(Tag).Assembly,
                    typeof(global::UmbracoExamine.BaseUmbracoIndexer).Assembly
			    };

		}

        [Test]
		public void Find_Class_Of_Type_With_Attribute()
		{

			var typesFound = TypeFinder.FindClassesOfTypeWithAttribute<TestEditor, MyTestAttribute>(_assemblies);
			Assert.AreEqual(2, typesFound.Count());
		}

        [Test]
        public void Find_Classes_Of_Type()
        {
            var typesFound = TypeFinder.FindClassesOfType<IApplicationStartupHandler>(_assemblies);            
            var originalTypesFound = TypeFinderOriginal.FindClassesOfType<IApplicationStartupHandler>(_assemblies);

            Assert.AreEqual(originalTypesFound.Count(), typesFound.Count());
            Assert.AreEqual(5, typesFound.Count());
            Assert.AreEqual(5, originalTypesFound.Count());
        }

        [Test]
        public void Find_Classes_With_Attribute()
        {
            var typesFound = TypeFinder.FindClassesWithAttribute<RestExtensionAttribute>(_assemblies);
            Assert.AreEqual(1, typesFound.Count());
        }

        [Ignore]
        [Test]
        public void Benchmark_Original_Finder()
        {
            using (DisposableTimer.TraceDuration<TypeFinderTests>("Starting test", "Finished test"))
            {
                using (DisposableTimer.TraceDuration<TypeFinderTests>("Starting FindClassesOfType", "Finished FindClassesOfType"))
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        Assert.Greater(TypeFinderOriginal.FindClassesOfType<DisposableObject>(_assemblies).Count(), 0);
                    }
                }
                using (DisposableTimer.TraceDuration<TypeFinderTests>("Starting FindClassesOfTypeWithAttribute", "Finished FindClassesOfTypeWithAttribute"))
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        Assert.Greater(TypeFinderOriginal.FindClassesOfTypeWithAttribute<TestEditor, MyTestAttribute>(_assemblies).Count(), 0);
                    }
                }
                using (DisposableTimer.TraceDuration<TypeFinderTests>("Starting FindClassesWithAttribute", "Finished FindClassesWithAttribute"))
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        Assert.Greater(TypeFinderOriginal.FindClassesWithAttribute<XsltExtensionAttribute>(_assemblies).Count(), 0);
                    }
                }
            }
            
        }

        [Ignore]
        [Test]
        public void Benchmark_New_Finder()
        {
            using (DisposableTimer.TraceDuration<TypeFinderTests>("Starting test", "Finished test"))
            {
                using (DisposableTimer.TraceDuration<TypeFinderTests>("Starting FindClassesOfType", "Finished FindClassesOfType"))
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        Assert.Greater(TypeFinder.FindClassesOfType<DisposableObject>(_assemblies).Count(), 0);
                    }
                }
                using (DisposableTimer.TraceDuration<TypeFinderTests>("Starting FindClassesOfTypeWithAttribute", "Finished FindClassesOfTypeWithAttribute"))
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        Assert.Greater(TypeFinder.FindClassesOfTypeWithAttribute<TestEditor, MyTestAttribute>(_assemblies).Count(), 0);
                    }
                }
                using (DisposableTimer.TraceDuration<TypeFinderTests>("Starting FindClassesWithAttribute", "Finished FindClassesWithAttribute"))
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        Assert.Greater(TypeFinder.FindClassesWithAttribute<XsltExtensionAttribute>(_assemblies).Count(), 0);
                    }
                }
            }
            
        }

        public class MyTag : ITag
        {
            public int Id { get; private set; }
            public string TagCaption { get; private set; }
            public string Group { get; private set; }
        }

        public class MySuperTag : MyTag
        {
            
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        public class MyTestAttribute : Attribute
        {

        }

        public abstract class TestEditor
        {

        }

        [MyTest]
        public class BenchmarkTestEditor : TestEditor
        {

        }

        [MyTest]
        public class MyOtherTestEditor : TestEditor
        {

        }

        //USED FOR THE ABOVE TESTS
        // see this issue for details: http://issues.umbraco.org/issue/U4-1187
        internal static class TypeFinderOriginal
        {

            private static readonly ConcurrentBag<Assembly> LocalFilteredAssemblyCache = new ConcurrentBag<Assembly>();
            private static readonly ReaderWriterLockSlim LocalFilteredAssemblyCacheLocker = new ReaderWriterLockSlim();
            private static ReadOnlyCollection<Assembly> _allAssemblies = null;
            private static ReadOnlyCollection<Assembly> _binFolderAssemblies = null;
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
            internal static IEnumerable<Assembly> GetAllAssemblies()
            {
                if (_allAssemblies == null)
                {
                    using (new WriteLock(Locker))
                    {
                        List<Assembly> assemblies = null;
                        try
                        {
                            var isHosted = HttpContext.Current != null;

                            try
                            {
                                if (isHosted)
                                {
                                    assemblies = new List<Assembly>(BuildManager.GetReferencedAssemblies().Cast<Assembly>());
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
                                var binFolder = Assembly.GetExecutingAssembly().GetAssemblyFile().Directory;
                                var binAssemblyFiles = Directory.GetFiles(binFolder.FullName, "*.dll", SearchOption.TopDirectoryOnly).ToList();
                                assemblies = new List<Assembly>();
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
                                assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().ToList());
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
                            _allAssemblies = new ReadOnlyCollection<Assembly>(assemblies);

                        }
                        catch (InvalidOperationException e)
                        {
                            if (!(e.InnerException is SecurityException))
                                throw;

                            _binFolderAssemblies = _allAssemblies;
                        }
                    }
                }

                return _allAssemblies;
            }

            /// <summary>
            /// Returns only assemblies found in the bin folder that have been loaded into the app domain.
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// This will be used if we implement App_Plugins from Umbraco v5 but currently it is not used.
            /// </remarks>
            internal static IEnumerable<Assembly> GetBinAssemblies()
            {

                if (_binFolderAssemblies == null)
                {
                    using (new WriteLock(Locker))
                    {
                        var assemblies = GetAssembliesWithKnownExclusions().ToArray();
                        var binFolder = Assembly.GetExecutingAssembly().GetAssemblyFile().Directory;
                        var binAssemblyFiles = Directory.GetFiles(binFolder.FullName, "*.dll", SearchOption.TopDirectoryOnly).ToList();
                        var domainAssemblyNames = binAssemblyFiles.Select(AssemblyName.GetAssemblyName);
                        var safeDomainAssemblies = new List<Assembly>();
                        var binFolderAssemblies = new List<Assembly>();

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
                                var foundAssembly = safeDomainAssemblies.FirstOrDefault(a => a.GetAssemblyFile() == assemblyName.GetAssemblyFile());
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

                        _binFolderAssemblies = new ReadOnlyCollection<Assembly>(binFolderAssemblies);
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
            internal static IEnumerable<Assembly> GetAssembliesWithKnownExclusions(
                IEnumerable<Assembly> excludeFromResults = null)
            {
                if (LocalFilteredAssemblyCache.Any()) return LocalFilteredAssemblyCache;
                using (new WriteLock(LocalFilteredAssemblyCacheLocker))
                {
                    var assemblies = GetFilteredAssemblies(excludeFromResults, KnownAssemblyExclusionFilter);
                    assemblies.ForEach(LocalFilteredAssemblyCache.Add);
                }
                return LocalFilteredAssemblyCache;
            }

            /// <summary>
            /// Return a list of found local Assemblies and exluding the ones passed in and excluding the exclusion list filter
            /// </summary>
            /// <param name="excludeFromResults"></param>
            /// <param name="exclusionFilter"></param>
            /// <returns></returns>
            private static IEnumerable<Assembly> GetFilteredAssemblies(
                IEnumerable<Assembly> excludeFromResults = null,
                string[] exclusionFilter = null)
            {
                if (excludeFromResults == null)
                    excludeFromResults = new List<Assembly>();
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
					"umbraco.providers,",
					"Umbraco.Web.UI,",
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
                    /* Mono */
                    "MonoDevelop.NUnit"
                };

            public static IEnumerable<Type> FindClassesOfTypeWithAttribute<T, TAttribute>()
                where TAttribute : Attribute
            {
                return FindClassesOfTypeWithAttribute<T, TAttribute>(GetAssembliesWithKnownExclusions(), true);
            }

            public static IEnumerable<Type> FindClassesOfTypeWithAttribute<T, TAttribute>(IEnumerable<Assembly> assemblies)
                where TAttribute : Attribute
            {
                return FindClassesOfTypeWithAttribute<T, TAttribute>(assemblies, true);
            }

            public static IEnumerable<Type> FindClassesOfTypeWithAttribute<T, TAttribute>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses)
                where TAttribute : Attribute
            {
                if (assemblies == null) throw new ArgumentNullException("assemblies");

                var l = new List<Type>();
                foreach (var a in assemblies)
                {
                    var types = from t in GetTypesWithFormattedException(a)
                                where !t.IsInterface
                                      && typeof(T).IsAssignableFrom(t)
                                      && t.GetCustomAttributes<TAttribute>(false).Any()
                                      && (!onlyConcreteClasses || (t.IsClass && !t.IsAbstract))
                                select t;
                    l.AddRange(types);
                }

                return l;
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

                return GetAssignablesFromType<T>(assemblies, onlyConcreteClasses);
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
            /// Finds the classes with attribute.
            /// </summary>
            /// <param name="type">The attribute type </param>
            /// <param name="assemblies">The assemblies.</param>
            /// <param name="onlyConcreteClasses">if set to <c>true</c> only concrete classes.</param>
            /// <returns></returns>
            public static IEnumerable<Type> FindClassesWithAttribute(Type type, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses)
            {
                if (assemblies == null) throw new ArgumentNullException("assemblies");
                if (!TypeHelper.IsTypeAssignableFrom<Attribute>(type))
                    throw new ArgumentException("The type specified: " + type + " is not an Attribute type");

                var l = new List<Type>();
                foreach (var a in assemblies)
                {
                    var types = from t in GetTypesWithFormattedException(a)
                                where !t.IsInterface && t.GetCustomAttributes(type, false).Any() && (!onlyConcreteClasses || (t.IsClass && !t.IsAbstract))
                                select t;
                    l.AddRange(types);
                }

                return l;
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
            /// Gets a collection of assignables of type T from a collection of assemblies
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assemblies"></param>
            /// <param name="onlyConcreteClasses"></param>
            /// <returns></returns>
            private static IEnumerable<Type> GetAssignablesFromType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses)
            {
                return GetTypes(typeof(T), assemblies, onlyConcreteClasses);
            }

            private static IEnumerable<Type> GetTypes(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses)
            {
                var l = new List<Type>();
                foreach (var a in assemblies)
                {
                    var types = from t in GetTypesWithFormattedException(a)
                                where !t.IsInterface && assignTypeFrom.IsAssignableFrom(t) && (!onlyConcreteClasses || (t.IsClass && !t.IsAbstract))
                                select t;
                    l.AddRange(types);
                }
                return l;
            }

            private static IEnumerable<Type> GetTypesWithFormattedException(Assembly a)
            {
                //if the assembly is dynamic, do not try to scan it
                if (a.IsDynamic)
                    return Enumerable.Empty<Type>();

                try
                {
                    return a.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Could not load types from assembly " + a.FullName + ", errors:");
                    foreach (var loaderException in ex.LoaderExceptions.WhereNotNull())
                    {
                        sb.AppendLine("Exception: " + loaderException.ToString());
                    }
                    throw new ReflectionTypeLoadException(ex.Types, ex.LoaderExceptions, sb.ToString());
                }
            }

            #endregion



        }
	}

    
}