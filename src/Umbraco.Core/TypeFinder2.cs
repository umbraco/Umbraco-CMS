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

namespace Umbraco.Core
{
	/// <summary>
	/// A utility class to find all classes of a certain type by reflection in the current bin folder
	/// of the web application. 
	/// </summary>
	public class TypeFinder2
	{
		private static readonly ConcurrentDictionary<Tuple<Type, Type>, bool> TypeCheckCache = new ConcurrentDictionary<Tuple<Type, Type>, bool>();
		private static readonly ConcurrentDictionary<Type, bool> ValueTypeCache = new ConcurrentDictionary<Type, bool>();
		private static readonly ConcurrentDictionary<Type, bool> ImplicitValueTypeCache = new ConcurrentDictionary<Type, bool>();
		//private static readonly ConcurrentDictionary<Type, FieldInfo[]> GetFieldsCache = new ConcurrentDictionary<Type, FieldInfo[]>();
		//private static readonly ConcurrentDictionary<Tuple<Type, bool, bool, bool>, PropertyInfo[]> GetPropertiesCache = new ConcurrentDictionary<Tuple<Type, bool, bool, bool>, PropertyInfo[]>();
		private static readonly ConcurrentBag<Assembly> LocalFilteredAssemblyCache = new ConcurrentBag<Assembly>();
		private static readonly ReaderWriterLockSlim LocalFilteredAssemblyCacheLocker = new ReaderWriterLockSlim();
		/// <summary>
		/// Caches attributed assembly information so they don't have to be re-read
		/// </summary>
		private readonly AttributedAssemblyList _attributedAssemblies = new AttributedAssemblyList();

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
					try
					{
						var isHosted = HttpContext.Current != null;

						try
						{
							if (isHosted)
							{
								_allAssemblies = new ReadOnlyCollection<Assembly>(
									BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToList());
							}
						}
						catch (InvalidOperationException e)
						{
							if (!(e.InnerException is SecurityException))
								throw;
						}

						_allAssemblies = _allAssemblies ?? new ReadOnlyCollection<Assembly>(AppDomain.CurrentDomain.GetAssemblies().ToList());

						var codeBase = Assembly.GetExecutingAssembly().CodeBase;
						var uri = new Uri(codeBase);
						var path = uri.LocalPath;
						var binFolder = new DirectoryInfo(Path.GetDirectoryName(path));

						var dllFiles = Directory.GetFiles(binFolder.FullName, "*.dll",
							SearchOption.TopDirectoryOnly).ToList();

						var binFolderAssemblies = dllFiles.Select(AssemblyName.GetAssemblyName)
							.Select(assemblyName =>
								{
									try
									{
										return _allAssemblies.FirstOrDefault(a => 
											AssemblyName.ReferenceMatchesDefinition(a.GetName(), assemblyName));
									}
									catch (SecurityException)
									{
										return null;
									}
								})
							.Where(locatedAssembly => locatedAssembly != null)
							.ToList();

						_binFolderAssemblies = new ReadOnlyCollection<Assembly>(binFolderAssemblies);
					}
					catch (InvalidOperationException e)
					{
						if (e.InnerException is SecurityException)
						{
							// Cannot scan bin folder in medium trust
							_binFolderAssemblies = _allAssemblies;
						}
					}
				}
			}

			return _allAssemblies;
		}

		/// <summary>
		/// Returns all assemblies loaded in the bin folder that are not in GAC
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<Assembly> GetBinFolderAssemblies()
		{
			if (_binFolderAssemblies == null) GetAllAssemblies();
			return _binFolderAssemblies;
		}

		/// <summary>
		/// Return a list of found assemblies for the AppDomain, excluding the GAC, the ones passed in and excluding the exclusion list filter
		/// </summary>
		/// <param name="excludeFromResults"></param>
		/// <returns></returns>
		internal static IEnumerable<Assembly> GetFilteredDomainAssemblies(IEnumerable<Assembly> excludeFromResults = null)
		{
			if (excludeFromResults == null)
				excludeFromResults = new List<Assembly>();

			return GetLocalAssembliesWithKnownExclusions().Except(excludeFromResults);
		}
		
		internal static IEnumerable<Assembly> GetLocalAssembliesWithKnownExclusions(IEnumerable<Assembly> excludeFromResults = null)
		{
			if (LocalFilteredAssemblyCache.Any()) return LocalFilteredAssemblyCache;
			using (new WriteLock(LocalFilteredAssemblyCacheLocker))
			{
				var assemblies = GetAllAssemblies()
					.Where(x => !x.GlobalAssemblyCache
						&& !KnownAssemblyExclusionFilter.Any(f => x.FullName.StartsWith(f)));
				assemblies.ForEach(LocalFilteredAssemblyCache.Add);
			}
			return LocalFilteredAssemblyCache;
		}

		/// <summary>
		/// Return a list of found local Assemblies in the bin folder exluding the ones passed in and excluding the exclusion list filter
		/// </summary>
		/// <param name="excludeFromResults"></param>
		/// <returns></returns>
		internal static IEnumerable<Assembly> GetFilteredBinFolderAssemblies(IEnumerable<Assembly> excludeFromResults = null)
		{
			if (excludeFromResults == null)
				excludeFromResults = new List<Assembly>();

			return GetBinFolderAssemblies()
				.Where(x => !excludeFromResults.Contains(x)
							&& !x.GlobalAssemblyCache
							&& !KnownAssemblyExclusionFilter.Any(f => x.FullName.StartsWith(f)));
		}

		/// <summary>
		/// Return a list of found local Assemblies including plugins and exluding the ones passed in and excluding the exclusion list filter
		/// </summary>
		/// <param name="excludeFromResults"></param>
		/// <param name="exclusionFilter"></param>
		/// <returns></returns>
		internal static IEnumerable<Assembly> GetFilteredLocalAssemblies(IEnumerable<Assembly> excludeFromResults = null,
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
                    "Umbraco.Core,",
                    "umbraco.datalayer,",
                    "umbraco.editorControls,",
                    "umbraco.interfaces,",
					"umbraco.MacroEngines,",
					"umbraco.MacroEngines.",
					"umbraco.macroRenderings,",
					"umbraco.providers,",
					"Umbraco.Web.UI,",
					"umbraco.webservices",
                    "Lucene.",
                    "Examine,",
                    "Examine."
                };

		/// <summary>
		/// Determines whether the type <paramref name="implementation"/> is assignable from the specified implementation <typeparamref name="TContract"/>,
		/// and caches the result across the application using a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TContract">The type of the contract.</typeparam>
		/// <param name="implementation">The implementation.</param>
		public static bool IsTypeAssignableFrom<TContract>(Type implementation)
		{
			return IsTypeAssignableFrom(typeof(TContract), implementation);
		}

		/// <summary>
		/// Determines whether the type <paramref name="implementation"/> is assignable from the specified implementation <typeparamref name="TContract"/>,
		/// and caches the result across the application using a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <param name="contract">The type of the contract.</param>
		/// <param name="implementation">The implementation.</param>
		/// <returns>
		/// 	<c>true</c> if [is type assignable from] [the specified contract]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsTypeAssignableFrom(Type contract, Type implementation)
		{
			// NOTE The use of a Tuple<,> here is because its Equals / GetHashCode implementation is literally 10.5x faster than KeyValuePair<,>
			return TypeCheckCache.GetOrAdd(new Tuple<Type, Type>(contract, implementation), x => x.Item1.IsAssignableFrom(x.Item2));
		}

		/// <summary>
		/// A cached method to determine whether <paramref name="implementation"/> represents a value type.
		/// </summary>
		/// <param name="implementation">The implementation.</param>
		public static bool IsValueType(Type implementation)
		{
			return ValueTypeCache.GetOrAdd(implementation, x => x.IsValueType || x.IsPrimitive);
		}

		/// <summary>
		/// A cached method to determine whether <paramref name="implementation"/> is an implied value type (<see cref="Type.IsValueType"/>, <see cref="Type.IsEnum"/> or a string).
		/// </summary>
		/// <param name="implementation">The implementation.</param>
		public static bool IsImplicitValueType(Type implementation)
		{
			return ImplicitValueTypeCache.GetOrAdd(implementation, x => IsValueType(implementation) || implementation.IsEnum || implementation == typeof(string));
		}

		public static bool IsTypeAssignableFrom<TContract>(object implementation)
		{
			if (implementation == null) throw new ArgumentNullException("implementation");
			return IsTypeAssignableFrom<TContract>(implementation.GetType());
		}



		/// <summary>
		/// Returns assemblies found in the specified path that the the specified custom attribute type applied to them
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="assemblies">The assemblies to search on</param>
		/// <returns></returns>
		internal IEnumerable<Assembly> FindAssembliesWithAttribute<T>(IEnumerable<Assembly> assemblies)
			where T : Attribute
		{

			var foundAssemblies = (from a in assemblies
								   //if its already registered, then ignore
								   where !_attributedAssemblies.IsRegistered<T>(a)
								   let customAttributes = a.GetCustomAttributes(typeof(T), false)
								   where customAttributes.Any()
								   select a).ToList();
			//now update the cache
			foreach (var a in foundAssemblies)
			{
				_attributedAssemblies.Add(new AttributedAssembly { Assembly = a, PluginAttributeType = typeof(T), AssemblyFolder = null });
			}

			//We must do a ToList() here because it is required to be serializable when using other app domains.
			return _attributedAssemblies
				.Where(x => x.PluginAttributeType.Equals(typeof(T))
					&& assemblies.Select(y => y.FullName).Contains(x.Assembly.FullName))
				.Select(x => x.Assembly)
				.ToList();
		}

		/// <summary>
		/// Returns found types in assemblies attributed with the specifed attribute type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TAssemblyAttribute"></typeparam>
		/// <param name="assemblies">The assemblies to search on</param>
		/// <returns></returns>
		internal IEnumerable<Type> FindClassesOfType<T, TAssemblyAttribute>(IEnumerable<Assembly> assemblies)
			where TAssemblyAttribute : Attribute
		{
			var found = FindAssembliesWithAttribute<TAssemblyAttribute>(assemblies);
			return GetAssignablesFromType<T>(found, true);
		}

		/// <summary>
		/// Returns found types in an assembly attributed with the specifed attribute type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TAssemblyAttribute">The type of the assembly attribute.</typeparam>
		/// <param name="assembly">The assembly.</param>
		/// <returns></returns>
		/// <remarks></remarks>
		internal IEnumerable<Type> FindClassesOfType<T, TAssemblyAttribute>(Assembly assembly)
			where TAssemblyAttribute : Attribute
		{
			return FindClassesOfType<T, TAssemblyAttribute>(new[] { assembly });
		}

		/// <summary>
		/// Searches all filtered local assemblies specified for classes of the type passed in.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IEnumerable<Type> FindClassesOfType<T>()
		{
			return FindClassesOfType<T>(GetFilteredLocalAssemblies(), true);
		}

		/// <summary>
		/// Searches all assemblies specified for classes of the type passed in.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="assemblyFiles"></param>
		/// <returns></returns>
		public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<string> assemblyFiles)
		{
			return FindClassesOfType<T>(assemblyFiles, true);
		}

		/// <summary>
		/// Searches all assemblies specified for classes of the type passed in.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="assemblyFiles"></param>
		/// <param name="onlyConcreteClasses">Only resolve concrete classes that can be instantiated</param>
		/// <returns></returns>
		public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<string> assemblyFiles, bool onlyConcreteClasses)
		{
			if (assemblyFiles == null) throw new ArgumentNullException("assemblyFiles");

			var typeAndAssembly = GetAssignablesFromType<T>(assemblyFiles, onlyConcreteClasses);
			return GetTypesFromResult(typeAndAssembly);
		}

		/// <summary>
		/// Returns all types found of in the assemblies specified of type T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="assemblies"></param>
		/// <param name="onlyConcreteClasses"></param>
		/// <returns></returns>
		public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses)
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
		public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies)
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
		public IEnumerable<Type> FindClassesWithAttribute<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses)
			where T : Attribute
		{
			if (assemblies == null) throw new ArgumentNullException("assemblies");

			return (from a in assemblies
					from t in GetTypesWithFormattedException(a)
					where !t.IsInterface && t.GetCustomAttributes<T>(false).Any() && (!onlyConcreteClasses || (t.IsClass && !t.IsAbstract))
					select t).ToList();
		}

		/// <summary>
		/// Finds the classes with attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="assemblies">The assemblies.</param>
		/// <returns></returns>
		public IEnumerable<Type> FindClassesWithAttribute<T>(IEnumerable<Assembly> assemblies)
			where T : Attribute
		{
			return FindClassesWithAttribute<T>(assemblies, true);
		}

		/// <summary>
		/// Finds the classes with attribute in filtered local assemblies
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IEnumerable<Type> FindClassesWithAttribute<T>()
			where T : Attribute
		{
			return FindClassesWithAttribute<T>(GetFilteredLocalAssemblies());
		}

		#region Internal Methods - For testing
		/// <summary>
		/// Used to find all types in all assemblies in the specified folder
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="folder"></param>
		/// <param name="onlyConcreteClasses"></param>
		/// <returns></returns>
		/// <remarks>
		/// This has potential cost alot in performance so it is marked internal and should ONLY be used
		/// where absolutely necessary (i.e. Tests)
		/// </remarks>
		internal IEnumerable<Type> FindClassesOfType<T>(DirectoryInfo folder, bool onlyConcreteClasses)
		{
			var types = new List<Type>();
			types.AddRange(FindClassesOfType<T>(folder.GetFiles("*.dll").Select(x => x.FullName)));
			return types;
		}

		/// <summary>
		/// Used to find all types in all assemblies in the specified folder
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="folder"></param>
		/// <returns></returns>
		/// <remarks>
		/// This has potential cost alot in performance so it is marked internal and should ONLY be used
		/// where absolutely necessary (i.e. Tests)
		/// </remarks>
		internal IEnumerable<Type> FindClassesOfType<T>(DirectoryInfo folder)
		{
			return FindClassesOfType<T>(folder, true);
		}
		#endregion

		#region Internal Attributed Assembly class
		private class AttributedAssembly
		{
			internal DirectoryInfo AssemblyFolder { get; set; }
			internal Assembly Assembly { get; set; }
			internal Type PluginAttributeType { get; set; }
		}
		private class AttributedAssemblyList : List<AttributedAssembly>
		{
			/// <summary>
			/// Determines if  that type has been registered with the folder
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="folder"></param>
			/// <returns></returns>
			internal bool IsRegistered<T>(DirectoryInfo folder)
			{
				return this.Where(x => x.PluginAttributeType == typeof(T)
											  && x.AssemblyFolder.FullName.ToUpper() == folder.FullName.ToUpper()).Any();
			}

			/// <summary>
			/// Determines if the assembly is already registered
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="assembly"></param>
			/// <returns></returns>
			internal bool IsRegistered<T>(Assembly assembly)
			{
				return this.Where(x => x.PluginAttributeType == typeof(T)
									   && x.Assembly.FullName.ToUpper() == assembly.FullName.ToUpper())
									   .Any();
			}
		}

		#endregion

		#region Private methods
		private static IEnumerable<Type> GetTypesFromResult(Dictionary<string, string> result)
		{
			return (from type in result
					let ass = Assembly.Load(type.Value)
					where ass != null
					select ass.GetType(type.Key, false)).ToList();
		}

		/// <summary>
		/// Gets a collection of assignables of type T from a collection of files
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="files">The files.</param>
		/// <param name="onlyConcreteClasses"></param>
		/// <returns></returns>
		private static Dictionary<string, string> GetAssignablesFromType<T>(IEnumerable<string> files, bool onlyConcreteClasses)
		{
			return GetTypes(typeof(T), files, onlyConcreteClasses);
		}

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
			return (from a in assemblies
					from t in GetTypesWithFormattedException(a)
					where !t.IsInterface && assignTypeFrom.IsAssignableFrom(t) && (onlyConcreteClasses ? (t.IsClass && !t.IsAbstract) : true)
					select t).ToList();
		}

		private static IEnumerable<Type> GetTypesWithFormattedException(Assembly a)
		{
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

		/// <summary>
		/// Returns of a collection of type names from a collection of assembky files.
		/// </summary>
		/// <param name="assignTypeFrom">The assign type.</param>
		/// <param name="assemblyFiles">The assembly files.</param>
		/// <param name="onlyConcreteClasses"></param>
		/// <returns></returns>
		private static Dictionary<string, string> GetTypes(Type assignTypeFrom, IEnumerable<string> assemblyFiles, bool onlyConcreteClasses)
		{
			var result = new Dictionary<string, string>();
			foreach (var assembly in
				assemblyFiles.Where(File.Exists).Select(Assembly.LoadFile).Where(assembly => assembly != null))
			{
				try
				{
					foreach (Type t in
						assembly.GetExportedTypes().Where(t => !t.IsInterface && assignTypeFrom.IsAssignableFrom(t) && (onlyConcreteClasses ? (t.IsClass && !t.IsAbstract) : true)))
					{
						//add the full type name and full assembly name                                  
						result.Add(t.FullName, t.Assembly.FullName);
					}
				}
				catch (ReflectionTypeLoadException ex)
				{
					Debug.WriteLine("Error reading assembly " + assembly.FullName + ": " + ex.Message);
				}
			}
			return result;
		}
		#endregion


		///// <summary>
		///// Gets (and caches) <see cref="FieldInfo"/> discoverable in the current <see cref="AppDomain"/> for a given <paramref name="type"/>.
		///// </summary>
		///// <param name="type">The source.</param>
		///// <returns></returns>
		//public static FieldInfo[] CachedDiscoverableFields(Type type)
		//{
		//    return GetFieldsCache.GetOrAdd(
		//        type,
		//        x => type
		//                 .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		//                 .Where(y => !y.IsInitOnly)
		//                 .ToArray());
		//}

		///// <summary>
		///// Gets (and caches) <see cref="PropertyInfo"/> discoverable in the current <see cref="AppDomain"/> for a given <paramref name="type"/>.
		///// </summary>
		///// <param name="type">The source.</param>
		///// <param name="mustRead">true if the properties discovered are readable</param>
		///// <param name="mustWrite">true if the properties discovered are writable</param>
		///// <param name="includeIndexed">true if the properties discovered are indexable</param>
		///// <returns></returns>
		//public static PropertyInfo[] CachedDiscoverableProperties(Type type, bool mustRead = true, bool mustWrite = true, bool includeIndexed = false)
		//{
		//    return GetPropertiesCache.GetOrAdd(
		//        new Tuple<Type, bool, bool, bool>(type, mustRead, mustWrite, includeIndexed),
		//        x => type
		//                 .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		//                 .Where(y => (!mustRead || y.CanRead)
		//                     && (!mustWrite || y.CanWrite)
		//                     && (includeIndexed || !y.GetIndexParameters().Any()))
		//                 .ToArray());
		//}
	}
}
