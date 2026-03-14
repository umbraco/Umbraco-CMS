// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Composing;

/// <summary>
///     Used for PluginTypeResolverTests
/// </summary>
internal static class TypeLoaderExtensions
{
    /// <summary>
    /// Resolves and returns all types that implement the <c>IFindMe</c> interface using the provided <see cref="TypeLoader"/>.
    /// </summary>
    /// <param name="resolver">The <see cref="TypeLoader"/> instance used to find the types.</param>
    /// <returns>An enumerable of <see cref="Type"/> instances that implement <c>IFindMe</c>.</returns>
    public static IEnumerable<Type> ResolveFindMeTypes(this TypeLoader resolver) =>
        resolver.GetTypes<TypeLoaderTests.IFindMe>();
}
