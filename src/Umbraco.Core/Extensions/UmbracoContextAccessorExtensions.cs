// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Web;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IUmbracoContextAccessor"/>.
/// </summary>
public static class UmbracoContextAccessorExtensions
{
    /// <summary>
    /// Gets the Umbraco context, throwing an exception if it is not available.
    /// </summary>
    /// <param name="umbracoContextAccessor">The Umbraco context accessor.</param>
    /// <returns>The current Umbraco context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the accessor is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the Umbraco context is not available.</exception>
    public static IUmbracoContext GetRequiredUmbracoContext(this IUmbracoContextAccessor umbracoContextAccessor)
    {
        if (umbracoContextAccessor == null)
        {
            throw new ArgumentNullException(nameof(umbracoContextAccessor));
        }

        if (!umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
        {
            throw new InvalidOperationException("Wasn't able to get an UmbracoContext");
        }

        return umbracoContext;
    }
}
