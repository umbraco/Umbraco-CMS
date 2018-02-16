using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Tests.Composing
{
    /// <summary>
    /// Used for PluginTypeResolverTests
    /// </summary>
    internal static class TypeLoaderExtensions
    {
        public static IEnumerable<Type> ResolveFindMeTypes(this TypeLoader resolver)
        {
            return resolver.GetTypes<TypeLoaderTests.IFindMe>();
        }
    }
}
