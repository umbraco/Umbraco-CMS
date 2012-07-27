using System;
using System.Collections.Generic;
using Umbraco.Core;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.cms
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginTypeResolverExtensions
	{		

		/// <summary>
		/// Returns all available IActionHandler in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveActions(this PluginTypeResolver resolver)
		{
			return resolver.ResolveTypes<IActionHandler>();
		}

	}
}