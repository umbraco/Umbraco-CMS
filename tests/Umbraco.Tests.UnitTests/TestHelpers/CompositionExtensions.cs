// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Tests.UnitTests.TestHelpers;

/// <summary>
/// Contains extension methods to assist with composition in unit tests for Umbraco CMS.
/// </summary>
public static class CompositionExtensions
{
    /// <summary>
    /// Creates a service provider from the given Umbraco builder.
    /// </summary>
    /// <param name="builder">The Umbraco builder to build the service provider from.</param>
    /// <returns>An IServiceProvider instance built from the builder's services.</returns>

    [Obsolete("This extension method exists only to ease migration, please refactor")]
    public static IServiceProvider CreateServiceProvider(this IUmbracoBuilder builder)
    {
        builder.Build();
        return builder.Services.BuildServiceProvider();
    }
}
