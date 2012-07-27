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

	//TODO: Get the App_Code stuff in here from the old one

	/// <summary>
	/// A utility class to find all classes of a certain type by reflection in the current bin folder
	/// of the web application. 
	/// </summary>
	public class TypeFinder2
	{
		
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
								_allAssemblies = new ReadOnlyCollection<Assembly>(BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToList());
							}
						}
						catch (InvalidOperationException e)
						{
							if (!(e.InnerException is SecurityException))
								throw;
						}

						_allAssemblies = _allAssemblies ?? new ReadOnlyCollection<Assembly>(AppDomain.CurrentDomain.GetAssemblies().ToList());

						//here we are getting just the /bin folder assemblies, we may need to use these if we implement the App_Plugins
						//stuff from v5.

						var codeBase = Assembly.GetExecutingAssembly().CodeBase;
						var uri = new Uri(codeBase);
						var path = uri.LocalPath;
						var binFolder = new DirectoryInfo(Path.GetDirectoryName(path));

						var dllFiles = Directory.GetFiles(binFolder.FullName, "*.dll",
							SearchOption.TopDirectoryOnly).ToList();

						var binFolderAssemblies = dllFiles.Select(AssemblyName.GetAssemblyName)
							.Select(assemblyName =>
									_allAssemblies.FirstOrDefault(a =>
																 AssemblyName.ReferenceMatchesDefinition(a.GetName(), assemblyName)))
							.Where(locatedAssembly => locatedAssembly != null)
							.ToList();

						_binFolderAssemblies = new ReadOnlyCollection<Assembly>(binFolderAssemblies);

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
				.Where(x => x.PluginAttributeType == typeof(T)
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

		public IEnumerable<Type> FindClassesOfTypeWithAttribute<T, TAttribute>()
			where TAttribute : Attribute
		{
			return FindClassesOfTypeWithAttribute<T, TAttribute>(GetAssembliesWithKnownExclusions(), true);
		}

		public IEnumerable<Type> FindClassesOfTypeWithAttribute<T, TAttribute>(IEnumerable<Assembly> assemblies)
			where TAttribute : Attribute
		{
			return FindClassesOfTypeWithAttribute<T, TAttribute>(assemblies, true);
		}

		public IEnumerable<Type> FindClassesOfTypeWithAttribute<T, TAttribute>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses)
			where TAttribute : Attribute
		{
			if (assemblies == null) throw new ArgumentNullException("assemblies");

			return (from a in assemblies
					from t in GetTypesWithFormattedException(a)
					where !t.IsInterface 
						&& typeof(T).IsAssignableFrom(t)
						&& t.GetCustomAttributes<TAttribute>(false).Any()
						&& (!onlyConcreteClasses || (t.IsClass && !t.IsAbstract))
					select t).ToList();
		}

		/// <summary>
		/// Searches all filtered local assemblies specified for classes of the type passed in.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IEnumerable<Type> FindClassesOfType<T>()
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
			return FindClassesWithAttribute<T>(GetAssembliesWithKnownExclusions());
		}


		#region Internal Attributed Assembly class

		//These can be removed once we implement the .hash file for caching assemblies that are found containing plugin types.
		// Once that is done, remove these classes and remove the TypeFinder test: Benchmark_Finding_First_Type_In_Assemblies

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
				return this.Any(x => x.PluginAttributeType == typeof(T)
				                     && x.AssemblyFolder.FullName.ToUpper() == folder.FullName.ToUpper());
			}

			/// <summary>
			/// Determines if the assembly is already registered
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="assembly"></param>
			/// <returns></returns>
			internal bool IsRegistered<T>(Assembly assembly)
			{
				return this.Any(x => x.PluginAttributeType == typeof(T)
				                     && x.Assembly.FullName.ToUpper() == assembly.FullName.ToUpper());
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
					where !t.IsInterface && assignTypeFrom.IsAssignableFrom(t) && (!onlyConcreteClasses || (t.IsClass && !t.IsAbstract))
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
						assembly.GetExportedTypes().Where(t => !t.IsInterface && assignTypeFrom.IsAssignableFrom(t) && (!onlyConcreteClasses || (t.IsClass && !t.IsAbstract))))
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


		
	}
}
