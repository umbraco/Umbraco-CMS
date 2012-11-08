using System;
using System.Collections.Generic;
using Umbraco.Core;
using umbraco.interfaces;

namespace umbraco.MacroEngines
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginManagerExtensions
	{

		/// <summary>
		/// Returns all available IMacroGuiRendering in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveRazorDataTypeModels(this PluginManager resolver)
		{
			return resolver.ResolveTypesWithAttribute<IRazorDataTypeModel, RazorDataTypeModel>();
		}

		
	}
}