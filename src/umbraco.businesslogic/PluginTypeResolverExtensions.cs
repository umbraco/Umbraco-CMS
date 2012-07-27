using System;
using System.Collections.Generic;
using Umbraco.Core;
using umbraco.interfaces;

namespace umbraco.businesslogic
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginTypeResolverExtensions
	{		
		/// <summary>
		/// Returns all available IApplicationStartupHandler objects
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveApplicationStartupHandlers(this PluginTypeResolver resolver)
		{
			return resolver.ResolveTypes<IApplicationStartupHandler>();
		}

		/// <summary>
		/// Returns all available IApplication in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveApplications(this PluginTypeResolver resolver)
		{
			return resolver.ResolveTypesWithAttribute<IApplication, ApplicationAttribute>();
		}

		/// <summary>
		/// Returns all available ITrees in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveTrees(this PluginTypeResolver resolver)
		{
			return resolver.ResolveTypesWithAttribute<ITree, TreeAttribute>();
		}

	}
}