using System;
using System.Collections.Generic;
using Umbraco.Core;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.media;
using umbraco.interfaces;

namespace umbraco.cms
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginManagerExtensions
	{

		/// <summary>
		/// Returns all available IDataType in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveMacroEngines(this PluginManager resolver)
		{
			return resolver.ResolveTypes<IMacroEngine>();
		}

	}
}