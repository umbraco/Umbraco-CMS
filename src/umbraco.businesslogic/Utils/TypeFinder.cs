using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Web;
using System.IO;
using System.Linq;
using System.Xml;
using Umbraco.Core;
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

		internal static IEnumerable<Type> FindClassesMarkedWithAttribute(Type attribute, IEnumerable<Assembly> assemblies)
		{
			List<Type> types = new List<Type>();
			bool searchGAC = false;

			foreach (Assembly assembly in assemblies)
			{
				// skip if the assembly is part of the GAC
				if (!searchGAC && assembly.GlobalAssemblyCache)
					continue;

				types.AddRange(FindClassesMarkedWithAttribute(assembly, attribute));
			}

			// also add types from app_code, if any

			var fileExtension = UmbracoSettings.AppCodeFileExtensionsList.ToArray();
			var appCodeFolder = new DirectoryInfo(IOHelper.MapPath(IOHelper.ResolveUrl("~/App_code")));
			if (appCodeFolder.Exists && appCodeFolder.GetFilesByExtensions(true, fileExtension).Any())
			{
				types.AddRange(FindClassesMarkedWithAttribute(Assembly.Load("App_Code"), attribute));
			}

			return types.Distinct();
		}

		/// <summary>
		/// Searches all loaded assemblies for classes marked with the attribute passed in.
		/// </summary>
		/// <returns>A list of found types</returns>
		public static IEnumerable<Type> FindClassesMarkedWithAttribute(Type attribute)
		{
			
			return FindClassesMarkedWithAttribute(attribute, AppDomain.CurrentDomain.GetAssemblies());
		}

		static IEnumerable<Type> FindClassesMarkedWithAttribute(Assembly assembly, Type attribute)
		{
			// DF: Fix Codeplex #30479 - Dynamic assemblies in Umbraco cause XSLTs to break - TypeFinder.cs
			// Just return if the assembly is dynamic.
			if (assembly.ManifestModule.GetType().Namespace == "System.Reflection.Emit") return new List<Type>();


			try
			{
				return assembly.GetExportedTypes().Where(type => type.GetCustomAttributes(attribute, true).Length > 0);
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
			return FindClassesOfType<T>(useSeperateAppDomain, onlyConcreteClasses, null);
		}


		static string GetAssemblyPath(Assembly ass)
		{
			var codeBase = ass.CodeBase;
			var uri = new Uri(codeBase);
			return uri.LocalPath;
		}

		internal static List<Type> FindClassesOfType<T>(bool useSeperateAppDomain, bool onlyConcreteClasses, IEnumerable<Assembly> assemblies)
		{
			if (useSeperateAppDomain)
			{
				string binFolder = Path.Combine(IO.IOHelper.MapPath("/", false), "bin");

				string[] strTypes;
				if (assemblies == null)
				{
					strTypes = TypeResolver.GetAssignablesFromType<T>(binFolder, "*.dll");	
				}
				else
				{
					strTypes = TypeResolver.GetAssignablesFromType<T>(assemblies.Select(GetAssemblyPath).ToArray());
				}
				
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
						foreach (Type type in System.Reflection.Assembly.Load("App_Code").GetExportedTypes())
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
		/// <param name="onlyConcreteClasses"> </param>
		/// <param name="assemblies"> </param>
		/// <returns>A list of found classes</returns>
		private static List<Type> FindClassesOfType(Type type, bool onlyConcreteClasses, IEnumerable<Assembly> assemblies = null)
		{
			bool searchGAC = false;

			List<Type> classesOfType = new List<Type>();

			Assembly[] a;
			if (assemblies == null)
			{
				a = AppDomain.CurrentDomain.GetAssemblies();
			}
			else
			{
				a = assemblies.ToArray();
			}

			foreach (Assembly assembly in a)
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

		[Obsolete("This method is no longer used and will be removed")]
		public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, bool searchSubdirs, params string[] extensions)
		{
			var allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
			IEnumerable<FileInfo> returnedFiles;
			returnedFiles = dir.GetFiles().Where(f => allowedExtensions.Contains(f.Extension));
			if (searchSubdirs)
			{
				foreach (DirectoryInfo di in dir.GetDirectories())
					returnedFiles.Concat(di.GetFilesByExtensions(true, extensions));
			}

			return returnedFiles;
		}

		[Obsolete("Use Umbraco.Core.SystemUtilities.GetCurrentTrustLevel() instead")]
		public static AspNetHostingPermissionLevel GetCurrentTrustLevel()
		{
			return Umbraco.Core.SystemUtilities.GetCurrentTrustLevel();
		}
	}
}
