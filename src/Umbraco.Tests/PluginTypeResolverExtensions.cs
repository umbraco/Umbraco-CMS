using System;
using System.Collections.Generic;
using Umbraco.Core;

namespace Umbraco.Tests
{
	/// <summary>
	/// Used for PluginTypeResolverTests
	/// </summary>
	internal static class PluginTypeResolverExtensions
	{
		public static IEnumerable<Type> ResolveFindMeTypes(this PluginTypeResolver resolver)
		{
			return resolver.ResolveTypes<PluginTypeResolverTests.IFindMe>();
		}
	}
}