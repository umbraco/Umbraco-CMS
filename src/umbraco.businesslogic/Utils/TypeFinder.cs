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
	[Obsolete("Use Umbraco.Core.TypeFinder instead")]
	public static class TypeFinder
	{
		/// <summary>
		/// Searches all loaded assemblies for classes marked with the attribute passed in.
		/// </summary>
		/// <returns>A list of found types</returns>
		[Obsolete("Use Umbraco.Core.TypeFinder.FindClassesWithAttribute instead")]
		public static IEnumerable<Type> FindClassesMarkedWithAttribute(Type attribute)
		{
			return Umbraco.Core.TypeFinder.FindClassesWithAttribute(
				attribute,
				Umbraco.Core.TypeFinder.GetAssembliesWithKnownExclusions(),
				true);
		}

		/// <summary>
		/// Searches all loaded assemblies for classes of the type passed in.
		/// </summary>
		/// <typeparam name="T">The type of object to search for</typeparam>
		/// <returns>A list of found types</returns>
		[Obsolete("Use Umbraco.Core.TypeFinder.FindClassesOfType instead")]
		public static List<Type> FindClassesOfType<T>()
		{
			return Umbraco.Core.TypeFinder.FindClassesOfType<T>().ToList();
		}

		/// <summary>
		/// Searches all loaded assemblies for classes of the type passed in.
		/// </summary>
		/// <typeparam name="T">The type of object to search for</typeparam>
		/// <param name="useSeperateAppDomain">true if a seperate app domain should be used to query the types. This is safer as it is able to clear memory after loading the types.</param>
		/// <returns>A list of found types</returns>
		[Obsolete("Use Umbraco.Core.TypeFinder.FindClassesOfType instead")]
		public static List<Type> FindClassesOfType<T>(bool useSeperateAppDomain)
		{
			return Umbraco.Core.TypeFinder.FindClassesOfType<T>().ToList();
		}

		/// <summary>
		/// Searches all loaded assemblies for classes of the type passed in.
		/// </summary>
		/// <typeparam name="T">The type of object to search for</typeparam>
		/// <param name="useSeperateAppDomain">true if a seperate app domain should be used to query the types. This is safer as it is able to clear memory after loading the types.</param>
		/// <param name="onlyConcreteClasses">True to only return classes that can be constructed</param>
		/// <returns>A list of found types</returns>
		[Obsolete("Use Umbraco.Core.TypeFinder.FindClassesOfType instead")]
		public static List<Type> FindClassesOfType<T>(bool useSeperateAppDomain, bool onlyConcreteClasses)
		{
			return Umbraco.Core.TypeFinder.FindClassesOfType<T>(
				Umbraco.Core.TypeFinder.GetAssembliesWithKnownExclusions(),
				onlyConcreteClasses).ToList();
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
