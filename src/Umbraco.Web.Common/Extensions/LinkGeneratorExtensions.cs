using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web.Mvc;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Extensions;

public static class LinkGeneratorExtensions
{
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
}
