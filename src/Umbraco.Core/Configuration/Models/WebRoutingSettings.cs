// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for web routing settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigWebRouting)]
public class WebRoutingSettings
{
    internal const bool StaticTryMatchingEndpointsForAllPages = false;
    internal const bool StaticTrySkipIisCustomErrors = false;
    internal const bool StaticInternalRedirectPreservesTemplate = false;
    internal const bool StaticDisableAlternativeTemplates = false;
    internal const bool StaticValidateAlternativeTemplates = false;
    internal const bool StaticDisableFindContentByIdPath = false;
    internal const bool StaticDisableFindContentByIdentifierPath = false;
    internal const bool StaticDisableRedirectUrlTracking = false;
    internal const string StaticUrlProviderMode = "Auto";
    internal const bool StaticUseStrictDomainMatching = false;

    /// <summary>
    ///     Gets or sets a value indicating whether to check if any routed endpoints match a front-end request before
    ///     the Umbraco dynamic router tries to map the request to an Umbraco content item.
    /// </summary>
    /// <remarks>
    ///     This should not be necessary if the Umbraco catch-all/dynamic route is registered last like it's supposed to be. In
    ///     that case
    ///     ASP.NET Core will automatically handle this in all cases. This is more of a backward compatible option since this
    ///     is what v7/v8 used
    ///     to do.
    /// </remarks>
    [DefaultValue(StaticTryMatchingEndpointsForAllPages)]
    public bool TryMatchingEndpointsForAllPages { get; set; } = StaticTryMatchingEndpointsForAllPages;

    /// <summary>
    ///     Gets or sets a value indicating whether IIS custom errors should be skipped.
    /// </summary>
    [DefaultValue(StaticTrySkipIisCustomErrors)]
    public bool TrySkipIisCustomErrors { get; set; } = StaticTrySkipIisCustomErrors;

    /// <summary>
    ///     Gets or sets a value indicating whether an internal redirect should preserve the template.
    /// </summary>
    [DefaultValue(StaticInternalRedirectPreservesTemplate)]
    public bool InternalRedirectPreservesTemplate { get; set; } = StaticInternalRedirectPreservesTemplate;

    /// <summary>
    ///     Gets or sets a value indicating whether the use of alternative templates are disabled.
    /// </summary>
    [DefaultValue(StaticDisableAlternativeTemplates)]
    public bool DisableAlternativeTemplates { get; set; } = StaticDisableAlternativeTemplates;

    /// <summary>
    ///     Gets or sets a value indicating whether the use of alternative templates should be validated.
    /// </summary>
    [DefaultValue(StaticValidateAlternativeTemplates)]
    public bool ValidateAlternativeTemplates { get; set; } = StaticValidateAlternativeTemplates;

    /// <summary>
    ///     Gets or sets a value indicating whether the content finder by a path of the content key (<see cref="Routing.ContentFinderByKeyPath" />) is disabled.
    /// </summary>
    [DefaultValue(StaticDisableFindContentByIdentifierPath)]
    public bool DisableFindContentByIdentifierPath { get; set; } = StaticDisableFindContentByIdentifierPath;

    /// <summary>
    ///     Gets or sets a value indicating whether redirect URL tracking is disabled.
    /// </summary>
    [DefaultValue(StaticDisableRedirectUrlTracking)]
    public bool DisableRedirectUrlTracking { get; set; } = StaticDisableRedirectUrlTracking;

    /// <summary>
    ///     Gets or sets a value for the URL provider mode (<see cref="UrlMode" />).
    /// </summary>
    [DefaultValue(StaticUrlProviderMode)]
    public UrlMode UrlProviderMode { get; set; } = Enum.Parse<UrlMode>(StaticUrlProviderMode);

    /// <summary>
    ///     Gets or sets a value for the Umbraco application URL.
    /// </summary>
    public string UmbracoApplicationUrl { get; set; } = null!;

    /// <summary>
    ///     Gets or sets a value indicating whether strict domain matching is used when finding content to match the request.
    /// </summary>
    /// <remarks>
    ///     <para>This setting is used within Umbraco's routing process based on content finders, specifically <see cref="Routing.ContentFinderByUrlNew" />.</para>
    ///     <para>If set to the default value of <see langword="false"/>, requests that don't match a configured domain will be routed to the first root node.</para>
    ///     <para>If set to <see langword="true"/>, requests that don't match a configured domain will not be routed.</para>
    /// </remarks>
    [DefaultValue(StaticUseStrictDomainMatching)]
    public bool UseStrictDomainMatching { get; set; } = StaticUseStrictDomainMatching;
}
