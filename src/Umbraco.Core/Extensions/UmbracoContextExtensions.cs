// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Web;

namespace Umbraco.Extensions;

public static class UmbracoContextExtensions
{
    /// <summary>
    ///     Boolean value indicating whether the current request is a front-end umbraco request
    /// </summary>
    public static bool IsFrontEndUmbracoRequest(this IUmbracoContext umbracoContext) =>
        umbracoContext.PublishedRequest != null;
}
