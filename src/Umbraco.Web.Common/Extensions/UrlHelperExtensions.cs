using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Web.Mvc;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Extensions;

public static class UrlHelperExtensions
{
    /// <summary>
    /// </summary>
    /// <returns></returns>
    public static string GetCacheBustHash(IHostingEnvironment hostingEnvironment, IUmbracoVersion umbracoVersion)
    {
        // make a hash of umbraco and client dependency version
        // in case the user bypasses the installer and just bumps the web.config or client dependency config

        // if in debug mode, always burst the cache
        if (hostingEnvironment.IsDebugMode)
        {
            return DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture).GenerateHash();
        }

        var version = umbracoVersion.SemanticVersion.ToSemanticString();
        return $"{version}".GenerateHash();
    }

    public static IHtmlContent GetCropUrl(this IUrlHelper urlHelper, IPublishedContent? mediaItem, string cropAlias, bool htmlEncode = true, UrlMode urlMode = UrlMode.Default)
    {
        if (mediaItem == null)
        {
            return HtmlString.Empty;
        }

        var url = mediaItem.GetCropUrl(cropAlias: cropAlias, useCropDimensions: true, urlMode: urlMode);
        return CreateHtmlString(url, htmlEncode);
    }

    public static IHtmlContent GetCropUrl(this IUrlHelper urlHelper, IPublishedContent? mediaItem, string propertyAlias, string cropAlias, bool htmlEncode = true, UrlMode urlMode = UrlMode.Default)
    {
        if (mediaItem == null)
        {
            return HtmlString.Empty;
        }

        var url = mediaItem.GetCropUrl(propertyAlias: propertyAlias, cropAlias: cropAlias, useCropDimensions: true, urlMode: urlMode);
        return CreateHtmlString(url, htmlEncode);
    }

    public static IHtmlContent GetCropUrl(
        this IUrlHelper urlHelper,
        IPublishedContent? mediaItem,
        int? width = null,
        int? height = null,
        string propertyAlias = Constants.Conventions.Media.File,
        string? cropAlias = null,
        int? quality = null,
        ImageCropMode? imageCropMode = null,
        ImageCropAnchor? imageCropAnchor = null,
        bool preferFocalPoint = false,
        bool useCropDimensions = false,
        bool cacheBuster = true,
        string? furtherOptions = null,
        bool htmlEncode = true,
        UrlMode urlMode = UrlMode.Default)
    {
        if (mediaItem == null)
        {
            return HtmlString.Empty;
        }

        var url = mediaItem.GetCropUrl(
            width,
            height,
            propertyAlias,
            cropAlias,
            quality,
            imageCropMode,
            imageCropAnchor,
            preferFocalPoint,
            useCropDimensions,
            cacheBuster,
            furtherOptions,
            urlMode);

        return CreateHtmlString(url, htmlEncode);
    }

    public static IHtmlContent GetCropUrl(
        this IUrlHelper urlHelper,
        ImageCropperValue? imageCropperValue,
        string cropAlias,
        int? width = null,
        int? height = null,
        int? quality = null,
        ImageCropMode? imageCropMode = null,
        ImageCropAnchor? imageCropAnchor = null,
        bool preferFocalPoint = false,
        bool useCropDimensions = true,
        string? cacheBusterValue = null,
        string? furtherOptions = null,
        bool htmlEncode = true)
    {
        if (imageCropperValue == null)
        {
            return HtmlString.Empty;
        }

        var imageUrl = imageCropperValue.Src;
        var url = imageUrl?.GetCropUrl(
            imageCropperValue,
            width,
            height,
            cropAlias,
            quality,
            imageCropMode,
            imageCropAnchor,
            preferFocalPoint,
            useCropDimensions,
            cacheBusterValue,
            furtherOptions);

        return CreateHtmlString(url, htmlEncode);
    }

    /// <summary>
    ///     Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified
    ///     SurfaceController
    /// </summary>
    /// <returns></returns>
    public static string SurfaceAction(
        this IUrlHelper url,
        IUmbracoContext umbracoContext,
        IDataProtectionProvider dataProtectionProvider,
        string action,
        string controllerName) =>
        url.SurfaceAction(umbracoContext, dataProtectionProvider, action, controllerName, null);

    /// <summary>
    ///     Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified
    ///     SurfaceController
    /// </summary>
    /// <returns></returns>
    public static string SurfaceAction(
        this IUrlHelper url,
        IUmbracoContext umbracoContext,
        IDataProtectionProvider dataProtectionProvider,
        string action,
        string controllerName,
        object? additionalRouteVals) =>
        url.SurfaceAction(
            umbracoContext,
            dataProtectionProvider,
            action,
            controllerName,
            string.Empty,
            additionalRouteVals);

    /// <summary>
    ///     Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified
    ///     SurfaceController
    /// </summary>
    /// <returns></returns>
    public static string SurfaceAction(
        this IUrlHelper url,
        IUmbracoContext umbracoContext,
        IDataProtectionProvider dataProtectionProvider,
        string action,
        string controllerName,
        string area,
        object? additionalRouteVals)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (string.IsNullOrEmpty(action))
        {
            throw new ArgumentException("Value can't be empty.", nameof(action));
        }

        if (controllerName == null)
        {
            throw new ArgumentNullException(nameof(controllerName));
        }

        if (string.IsNullOrEmpty(controllerName))
        {
            throw new ArgumentException("Value can't be empty.", nameof(controllerName));
        }

        var encryptedRoute = EncryptionHelper.CreateEncryptedRouteString(dataProtectionProvider, controllerName, action,
            area, additionalRouteVals);

        var result = umbracoContext.OriginalRequestUrl.AbsolutePath.EnsureEndsWith('?') + "ufprt=" + encryptedRoute;
        return result;
    }

    private static IHtmlContent CreateHtmlString(string? url, bool htmlEncode) =>
        htmlEncode ? new HtmlString(HttpUtility.HtmlEncode(url)) : new HtmlString(url);
}
