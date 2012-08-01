using System;
using System.Collections.Generic;
using Umbraco.Core;

namespace Umbraco.Tests
{
	/// <summary>
	/// Used for PluginTypeResolverTests
	/// </summary>
	internal static class PluginManagerExtensions
	{
		public static IEnumerable<Type> ResolveFindMeTypes(this PluginManager resolver)
		{
			return resolver.ResolveTypes<PluginManagerTests.IFindMe>();
		}
	}
}