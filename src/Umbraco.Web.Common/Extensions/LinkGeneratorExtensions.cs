using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Web.Mvc;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Extensions;

public static class LinkGeneratorExtensions
{
    /// <summary>
    /// Gets the Umbraco backoffice URL (if Umbraco is installed).
    /// </summary>
    /// <param name="linkGenerator">The link generator.</param>
    /// <returns>
    /// The Umbraco backoffice URL.
    /// </returns>
    public static string? GetUmbracoBackOfficeUrl(this LinkGenerator linkGenerator)
        => linkGenerator.GetPathByAction("Default", "BackOffice", new { area = Constants.Web.Mvc.BackOfficeArea });

    /// <summary>
    /// Gets the Umbraco backoffice URL (if Umbraco is installed) or application virtual path (in most cases just <c>"/"</c>).
    /// </summary>
    /// <param name="linkGenerator">The link generator.</param>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    /// <returns>
    /// The Umbraco backoffice URL.
    /// </returns>
    public static string GetUmbracoBackOfficeUrl(this LinkGenerator linkGenerator, IHostingEnvironment hostingEnvironment)
         => GetUmbracoBackOfficeUrl(linkGenerator) ?? hostingEnvironment.ApplicationVirtualPath;

    /// <summary>
    ///     Return the back office url if the back office is installed
    /// </summary>
    /// <remarks>
    /// This method contained a bug that would result in always returning "/".
    /// </remarks>
    [Obsolete("Use the GetUmbracoBackOfficeUrl extension method instead. This method will be removed in Umbraco 13.")]
    public static string? GetBackOfficeUrl(this LinkGenerator linkGenerator, IHostingEnvironment hostingEnvironment)
        => "/";

    /// <summary>
    ///     Return the Url for a Web Api service
    /// </summary>
    /// <typeparam name="T">The <see cref="UmbracoApiControllerBase" /></typeparam>
    public static string? GetUmbracoApiService<T>(this LinkGenerator linkGenerator, string actionName, object? id = null)
        where T : UmbracoApiControllerBase => linkGenerator.GetUmbracoControllerUrl(
        actionName,
        typeof(T),
        new Dictionary<string, object?> { ["id"] = id });

    public static string? GetUmbracoApiService<T>(this LinkGenerator linkGenerator, string actionName, IDictionary<string, object?>? values)
        where T : UmbracoApiControllerBase => linkGenerator.GetUmbracoControllerUrl(actionName, typeof(T), values);

    public static string? GetUmbracoApiServiceBaseUrl<T>(
        this LinkGenerator linkGenerator,
        Expression<Func<T, object?>> methodSelector)
        where T : UmbracoApiControllerBase
    {
        MethodInfo? method = ExpressionHelper.GetMethodInfo(methodSelector);
        if (method == null)
        {
            throw new MissingMethodException("Could not find the method " + methodSelector + " on type " + typeof(T) +
                                             " or the result ");
        }

        return linkGenerator.GetUmbracoApiService<T>(method.Name)?.TrimEnd(method.Name);
    }

    /// <summary>
    ///     Return the Url for an Umbraco controller
    /// </summary>
    public static string? GetUmbracoControllerUrl(this LinkGenerator linkGenerator, string actionName, string controllerName, string? area, IDictionary<string, object?>? dict = null)
    {
        if (actionName == null)
        {
            throw new ArgumentNullException(nameof(actionName));
        }

        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(actionName));
        }

        if (controllerName == null)
        {
            throw new ArgumentNullException(nameof(controllerName));
        }

        if (string.IsNullOrWhiteSpace(controllerName))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(controllerName));
        }

        if (dict is null)
        {
            dict = new Dictionary<string, object?>();
        }

        if (!area.IsNullOrWhiteSpace())
        {
            dict["area"] = area!;
        }

        IDictionary<string, object?> values = dict.Aggregate(
            new ExpandoObject() as IDictionary<string, object?>,
            (a, p) =>
            {
                a.Add(p.Key, p.Value);
                return a;
            });

        return linkGenerator.GetPathByAction(actionName, controllerName, values);
    }

    /// <summary>
    ///     Return the Url for an Umbraco controller
    /// </summary>
    public static string? GetUmbracoControllerUrl(this LinkGenerator linkGenerator, string actionName, Type controllerType, IDictionary<string, object?>? values = null)
    {
        if (actionName == null)
        {
            throw new ArgumentNullException(nameof(actionName));
        }

        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(actionName));
        }

        if (controllerType == null)
        {
            throw new ArgumentNullException(nameof(controllerType));
        }

        var area = string.Empty;

        if (!typeof(ControllerBase).IsAssignableFrom(controllerType))
        {
            throw new InvalidOperationException($"The controller {controllerType} is of type {typeof(ControllerBase)}");
        }

        PluginControllerMetadata metaData = PluginController.GetMetadata(controllerType);
        if (metaData.AreaName.IsNullOrWhiteSpace() == false)
        {
            // set the area to the plugin area
            area = metaData.AreaName;
        }

        return linkGenerator.GetUmbracoControllerUrl(actionName, ControllerExtensions.GetControllerName(controllerType), area, values);
    }

    public static string? GetUmbracoApiService<T>(
        this LinkGenerator linkGenerator,
        Expression<Func<T, object>> methodSelector)
        where T : UmbracoApiController
    {
        MethodInfo? method = ExpressionHelper.GetMethodInfo(methodSelector);
        IDictionary<string, object?>? methodParams = ExpressionHelper.GetMethodParams(methodSelector);
        if (method == null)
        {
            throw new MissingMethodException(
                $"Could not find the method {methodSelector} on type {typeof(T)} or the result ");
        }

        if (methodParams?.Any() == false)
        {
            return linkGenerator.GetUmbracoApiService<T>(method.Name);
        }

        return linkGenerator.GetUmbracoApiService<T>(method.Name, methodParams);
    }
}
