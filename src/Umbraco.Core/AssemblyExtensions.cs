using System;
using System.IO;
using System.Reflection;

namespace Umbraco.Core
{
	internal static class AssemblyExtensions
	{
		/// <summary>
		/// Returns the file used to load the assembly
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		public static FileInfo GetAssemblyFile(this Assembly assembly)
		{
			var codeBase = assembly.CodeBase;
			var uri = new Uri(codeBase);
			var path = uri.LocalPath;
			return new FileInfo(path);
		}

		/// <summary>
		///  Returns the file used to load the assembly
		/// </summary>
		/// <param name="assemblyName"></param>
		/// <returns></returns>
		public static FileInfo GetAssemblyFile(this AssemblyName assemblyName)
		{
			var codeBase = assemblyName.CodeBase;
			var uri = new Uri(codeBase);
			var path = uri.LocalPath;
			return new FileInfo(path);
		}

	}
}