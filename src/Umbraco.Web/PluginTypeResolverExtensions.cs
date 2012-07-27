using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;
using Umbraco.Web.Routing;


namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginTypeResolverExtensions
	{

		/// <summary>
		/// Returns all IDocumentLookup types
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveLookups(this PluginTypeResolver resolver)
		{
			return resolver.ResolveTypes<IDocumentLookup>();
		}

	}
}