using System;
using System.Collections.Generic;
using Umbraco.Core;
using umbraco.interfaces;

namespace umbraco.businesslogic
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginManagerExtensions
	{
		/// <summary>
		/// Returns all available IApplication in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveApplications(this PluginManager resolver)
		{			
			return resolver.ResolveTypesWithAttribute<IApplication, ApplicationAttribute>();
		}

		/// <summary>
		/// Returns all available ITrees in application that are attribute with TreeAttribute
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveAttributedTrees(this PluginManager resolver)
		{
			return resolver.ResolveTypesWithAttribute<ITree, TreeAttribute>();
		}
		
	}
}