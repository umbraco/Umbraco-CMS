using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco.Extensions;

public static class LinkGeneratorExtensions
{
    /// <summary>
    ///     Return the Url for a Web Api service
    /// </summary>
    /// <typeparam name="T">The <see cref="UmbracoApiControllerBase" /></typeparam>
    [Obsolete("This will be removed in Umbraco 15.")]
    public static string? GetUmbracoApiService<T>(this LinkGenerator linkGenerator, string actionName, object? id = null)
        where T : UmbracoApiControllerBase => linkGenerator.GetUmbracoControllerUrl(
        actionName,
        typeof(T),
        new Dictionary<string, object?> { ["id"] = id });

    [Obsolete("This will be removed in Umbraco 15.")]
    public static string? GetUmbracoApiService<T>(this LinkGenerator linkGenerator, string actionName, IDictionary<string, object?>? values)
        where T : UmbracoApiControllerBase => linkGenerator.GetUmbracoControllerUrl(actionName, typeof(T), values);

    [Obsolete("This will be removed in Umbraco 15.")]
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
    ///     Return the Url for a Surface Controller
    /// </summary>
    /// <typeparam name="T">The <see cref="SurfaceController" /></typeparam>
    public static string? GetUmbracoSurfaceUrl<T>(
        this LinkGenerator linkGenerator,
        Expression<Func<T, object>> methodSelector)
        where T : SurfaceController
    {
        MethodInfo? method = ExpressionHelper.GetMethodInfo(methodSelector);
        IDictionary<string, object?>? methodParams = ExpressionHelper.GetMethodParams(methodSelector);

        if (method == null)
        {
            throw new MissingMethodException(
                $"Could not find the method {methodSelector} on type {typeof(T)} or the result ");
        }

        if (methodParams is null || methodParams.Any() == false)
        {
            return linkGenerator.GetUmbracoSurfaceUrl<T>(method.Name);
        }

        return linkGenerator.GetUmbracoSurfaceUrl<T>(method.Name, methodParams);
    }

    /// <summary>
    ///     Return the Url for a Surface Controller
    /// </summary>
    /// <typeparam name="T">The <see cref="SurfaceController" /></typeparam>
    public static string? GetUmbracoSurfaceUrl<T>(this LinkGenerator linkGenerator, string actionName, object? id = null)
        where T : SurfaceController => linkGenerator.GetUmbracoControllerUrl(
        actionName,
        typeof(T),
        new Dictionary<string, object?> { ["id"] = id });

    [Obsolete("This will be removed in Umbraco 15.")]
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
