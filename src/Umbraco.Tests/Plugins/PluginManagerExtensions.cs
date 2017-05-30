using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Plugins;

namespace Umbraco.Tests.Plugins
{
	/// <summary>
	/// Used for PluginTypeResolverTests
	/// </summary>
	internal static class PluginManagerExtensions
	{
		public static IEnumerable<Type> ResolveFindMeTypes(this TypeLoader resolver)
		{
			return resolver.GetTypes<PluginManagerTests.IFindMe>();
		}
	}
}