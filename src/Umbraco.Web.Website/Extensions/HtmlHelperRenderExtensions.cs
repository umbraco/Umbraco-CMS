using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Web.Mvc;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Mvc;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Collections;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco.Extensions;

/// <summary>
///     HtmlHelper extensions for use in templates
/// </summary>
public static class HtmlHelperRenderExtensions
{
    private static T GetRequiredService<T>(IHtmlHelper htmlHelper)
        where T : notnull
        => GetRequiredService<T>(htmlHelper.ViewContext);

    private static T GetRequiredService<T>(ViewContext viewContext)
        where T : notnull
        => viewContext.HttpContext.RequestServices.GetRequiredService<T>();

    /// <summary>
    ///     Renders the markup for the profiler
    /// </summary>
    public static IHtmlContent RenderProfiler(this IHtmlHelper helper)
        => new HtmlString(GetRequiredService<IProfilerHtml>(helper).Render());

    /// <summary>
    ///     Renders a partial view that is found in the specified area
    /// </summary>
    public static IHtmlContent AreaPartial(
        this IHtmlHelper helper,
        string partial,
        string area,
        object? model = null,
        ViewDataDictionary? viewData = null)
    {
        var originalArea = helper.ViewContext.RouteData.DataTokens["area"];
        helper.ViewContext.RouteData.DataTokens["area"] = area;
        IHtmlContent? result = helper.Partial(partial, model, viewData);
        helper.ViewContext.RouteData.DataTokens["area"] = originalArea;
        return result;
    }

    /// <summary>
    ///     Will render the preview badge when in preview mode which is not required ever unless the MVC page you are
    ///     using does not inherit from UmbracoViewPage
    /// </summary>
    /// <remarks>
    ///     See: http://issues.umbraco.org/issue/U4-1614
    /// </remarks>
    public static IHtmlContent PreviewBadge(
        this IHtmlHelper helper,
        IUmbracoContextAccessor umbracoContextAccessor,
        IHttpContextAccessor httpContextAccessor,
        GlobalSettings globalSettings,
        IIOHelper ioHelper,
        ContentSettings contentSettings)
    {
        IHostingEnvironment hostingEnvironment = GetRequiredService<IHostingEnvironment>(helper);
        IUmbracoContext umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();

        if (umbracoContext.InPreviewMode)
        {
            var htmlBadge =
                string.Format(
                    contentSettings.PreviewBadge,
                    hostingEnvironment.ToAbsolute(globalSettings.UmbracoPath),
                    WebUtility.UrlEncode(httpContextAccessor.GetRequiredHttpContext().Request.Path),
                    umbracoContext.PublishedRequest?.PublishedContent?.Id);
            return new HtmlString(htmlBadge);
        }

        return HtmlString.Empty;
    }

    public static async Task<IHtmlContent?> CachedPartialAsync(
        this IHtmlHelper htmlHelper,
        string partialViewName,
        object model,
        TimeSpan cacheTimeout,
        bool cacheByPage = false,
        bool cacheByMember = false,
        ViewDataDictionary? viewData = null,
        Func<object, ViewDataDictionary?, string>? contextualKeyBuilder = null)
    {
        var cacheKey = new StringBuilder(partialViewName);

        // let's always cache by the current culture to allow variants to have different cache results
        var cultureName = Thread.CurrentThread.CurrentUICulture.Name;
        if (!string.IsNullOrEmpty(cultureName))
        {
            cacheKey.AppendFormat("{0}-", cultureName);
        }

        IUmbracoContextAccessor umbracoContextAccessor = GetRequiredService<IUmbracoContextAccessor>(htmlHelper);
        umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext);

        if (cacheByPage)
        {
            if (umbracoContext == null)
            {
                throw new InvalidOperationException(
                    "Cannot cache by page if the UmbracoContext has not been initialized, this parameter can only be used in the context of an Umbraco request");
            }

            cacheKey.AppendFormat("{0}-", umbracoContext.PublishedRequest?.PublishedContent?.Id ?? 0);
        }

        if (cacheByMember)
        {
            IMemberManager memberManager =
                htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IMemberManager>();
            MemberIdentityUser? currentMember = await memberManager.GetCurrentMemberAsync();
            cacheKey.AppendFormat("m{0}-", currentMember?.Id ?? "0");
        }

        if (contextualKeyBuilder != null)
        {
            var contextualKey = contextualKeyBuilder(model, viewData);
            cacheKey.AppendFormat("c{0}-", contextualKey);
        }

        AppCaches appCaches = GetRequiredService<AppCaches>(htmlHelper);
        IHostingEnvironment hostingEnvironment = GetRequiredService<IHostingEnvironment>(htmlHelper);

        return appCaches.CachedPartialView(
            hostingEnvironment,
            umbracoContext!,
            htmlHelper,
            partialViewName,
            model,
            cacheTimeout,
            cacheKey.ToString(),
            viewData);
    }

    // public static IHtmlContent EditorFor<T>(this IHtmlHelper htmlHelper, string templateName = "", string htmlFieldName = "", object additionalViewData = null)
    //     where T : new()
    // {
    //     var model = new T();
    //     htmlHelper.Contextualize(htmlHelper.ViewContext.CopyWithModel(model));
    //
    //     //
    //     // var typedHelper = new HtmlHelper<T>(htmlHelper.
    //     // htmlHelper.
    //     //     ,
    //     //     htmlHelper.ViewDataContainer.CopyWithModel(model));
    //
    //
    //
    //     return htmlHelper.EditorForModel(x => model, templateName, htmlFieldName, additionalViewData);
    // }

    /// <summary>
    ///     A validation summary that lets you pass in a prefix so that the summary only displays for elements
    ///     containing the prefix. This allows you to have more than on validation summary on a page.
    /// </summary>
    public static IHtmlContent ValidationSummary(
        this IHtmlHelper htmlHelper,
        string prefix = "",
        bool excludePropertyErrors = false,
        string message = "",
        object? htmlAttributes = null)
    {
        if (prefix.IsNullOrWhiteSpace())
        {
            return htmlHelper.ValidationSummary(excludePropertyErrors, message, htmlAttributes);
        }

        IHtmlGenerator htmlGenerator = GetRequiredService<IHtmlGenerator>(htmlHelper);

        ViewContext viewContext = htmlHelper.ViewContext.Clone();

        // change the HTML field name
        viewContext.ViewData.TemplateInfo.HtmlFieldPrefix = prefix;

        TagBuilder? tagBuilder = htmlGenerator.GenerateValidationSummary(
            viewContext,
            excludePropertyErrors,
            message,
            null,
            htmlAttributes);
        if (tagBuilder == null)
        {
            return HtmlString.Empty;
        }

        return tagBuilder;
    }

    /// <summary>
    ///     Returns the result of a child action of a strongly typed SurfaceController
    /// </summary>
    /// <typeparam name="T">The <see cref="SurfaceController" /></typeparam>
    public static IHtmlContent ActionLink<T>(this IHtmlHelper htmlHelper, string actionName)
        where T : SurfaceController => htmlHelper.ActionLink(actionName, typeof(T));

    /// <summary>
    ///     Returns the result of a child action of a SurfaceController
    /// </summary>
    public static IHtmlContent ActionLink(this IHtmlHelper htmlHelper, string actionName, Type surfaceType)
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

        if (surfaceType == null)
        {
            throw new ArgumentNullException(nameof(surfaceType));
        }

        SurfaceControllerTypeCollection surfaceControllerTypeCollection =
            GetRequiredService<SurfaceControllerTypeCollection>(htmlHelper);
        Type? surfaceController = surfaceControllerTypeCollection.SingleOrDefault(x => x == surfaceType);
        if (surfaceController == null)
        {
            throw new InvalidOperationException("Could not find the surface controller of type " +
                                                surfaceType.FullName);
        }

        var routeVals = new RouteValueDictionary(new { area = string.Empty });

        PluginControllerMetadata metaData = PluginController.GetMetadata(surfaceController);
        if (!metaData.AreaName.IsNullOrWhiteSpace())
        {
            // set the area to the plugin area
            if (routeVals.ContainsKey("area"))
            {
                routeVals["area"] = metaData.AreaName;
            }
            else
            {
                routeVals.Add("area", metaData.AreaName);
            }
        }

        return htmlHelper.ActionLink(actionName, metaData.ControllerName, routeVals);
    }

    /// <summary>
    ///     Outputs the hidden html input field for Surface Controller route information
    /// </summary>
    /// <typeparam name="TSurface">The <see cref="SurfaceController" /> type</typeparam>
    /// <remarks>
    ///     Typically not used directly because BeginUmbracoForm automatically outputs this value when routing
    ///     for surface controllers. But this could be used in case a form tag is manually created.
    /// </remarks>
    public static IHtmlContent SurfaceControllerHiddenInput<TSurface>(
        this IHtmlHelper htmlHelper,
        string controllerAction,
        string area,
        object? additionalRouteVals = null)
        where TSurface : SurfaceController
    {
        var inputField = GetSurfaceControllerHiddenInput(
            GetRequiredService<IDataProtectionProvider>(htmlHelper),
            ControllerExtensions.GetControllerName<TSurface>(),
            controllerAction,
            area,
            additionalRouteVals);

        return new HtmlString(inputField);
    }

    private static string GetSurfaceControllerHiddenInput(
        IDataProtectionProvider dataProtectionProvider,
        string controllerName,
        string controllerAction,
        string area,
        object? additionalRouteVals = null)
    {
        var encryptedString = EncryptionHelper.CreateEncryptedRouteString(
            dataProtectionProvider,
            controllerName,
            controllerAction,
            area,
            additionalRouteVals);

        return "<input name=\"ufprt\" type=\"hidden\" value=\"" + encryptedString + "\" />";
    }

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared
    ///     controller.
    /// </summary>
    /// <param name="html">The HTML helper.</param>
    /// <param name="action">Name of the action.</param>
    /// <param name="controllerName">Name of the controller.</param>
    /// <param name="method">The method.</param>
    /// <returns>the <see cref="MvcForm" /></returns>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        string controllerName,
        FormMethod method)
        => html.BeginUmbracoForm(action, controllerName, null, new Dictionary<string, object?>(), method);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
    /// </summary>
    public static MvcForm BeginUmbracoForm(this IHtmlHelper html, string action, string controllerName)
        => html.BeginUmbracoForm(action, controllerName, null, new Dictionary<string, object?>());

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        string controllerName,
        object additionalRouteVals,
        FormMethod method)
        => html.BeginUmbracoForm(action, controllerName, additionalRouteVals, new Dictionary<string, object?>(), method);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        string controllerName,
        object additionalRouteVals)
        => html.BeginUmbracoForm(action, controllerName, additionalRouteVals, new Dictionary<string, object?>());

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        string controllerName,
        object additionalRouteVals,
        object htmlAttributes,
        FormMethod method) =>
        html.BeginUmbracoForm(
            action,
            controllerName,
            additionalRouteVals,
            HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes),
            method);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        string controllerName,
        object additionalRouteVals,
        object htmlAttributes) =>
        html.BeginUmbracoForm(
            action,
            controllerName,
            additionalRouteVals,
            HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        string controllerName,
        object? additionalRouteVals,
        IDictionary<string, object?> htmlAttributes,
        FormMethod method)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(action));
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

        return html.BeginUmbracoForm(action, controllerName, string.Empty, additionalRouteVals, htmlAttributes, method);
    }

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        string controllerName,
        object? additionalRouteVals,
        IDictionary<string, object?> htmlAttributes)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(action));
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

        return html.BeginUmbracoForm(action, controllerName, string.Empty, additionalRouteVals, htmlAttributes);
    }

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(this IHtmlHelper html, string action, Type surfaceType, FormMethod method)
        => html.BeginUmbracoForm(action, surfaceType, null, new Dictionary<string, object?>(), method);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(this IHtmlHelper html, string action, Type surfaceType)
        => html.BeginUmbracoForm(action, surfaceType, null, new Dictionary<string, object?>());

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    /// <typeparam name="T">The <see cref="SurfaceController" /> type</typeparam>
    public static MvcForm BeginUmbracoForm<T>(this IHtmlHelper html, string action, FormMethod method)
        where T : SurfaceController => html.BeginUmbracoForm(action, typeof(T), method);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    /// <typeparam name="T">The <see cref="SurfaceController" /> type</typeparam>
    public static MvcForm BeginUmbracoForm<T>(this IHtmlHelper html, string action)
        where T : SurfaceController => html.BeginUmbracoForm(action, typeof(T));

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        Type surfaceType,
        object additionalRouteVals,
        FormMethod method) =>
        html.BeginUmbracoForm(
            action,
            surfaceType,
            additionalRouteVals,
            new Dictionary<string, object?>(),
            method);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        Type surfaceType,
        object additionalRouteVals) =>
        html.BeginUmbracoForm(action, surfaceType, additionalRouteVals, new Dictionary<string, object?>());

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    /// <typeparam name="T">The <see cref="SurfaceController" /> type</typeparam>
    public static MvcForm BeginUmbracoForm<T>(this IHtmlHelper html, string action, object additionalRouteVals, FormMethod method)
        where T : SurfaceController => html.BeginUmbracoForm(action, typeof(T), additionalRouteVals, method);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    /// <typeparam name="T">The <see cref="SurfaceController" /> type</typeparam>
    public static MvcForm BeginUmbracoForm<T>(this IHtmlHelper html, string action, object additionalRouteVals)
        where T : SurfaceController => html.BeginUmbracoForm(action, typeof(T), additionalRouteVals);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        Type surfaceType,
        object additionalRouteVals,
        object htmlAttributes,
        FormMethod method) =>
        html.BeginUmbracoForm(
            action,
            surfaceType,
            additionalRouteVals,
            HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes),
            method);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        Type surfaceType,
        object additionalRouteVals,
        object htmlAttributes) =>
        html.BeginUmbracoForm(
            action,
            surfaceType,
            additionalRouteVals,
            HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    /// <typeparam name="T">The <see cref="SurfaceController" /> type</typeparam>
    public static MvcForm BeginUmbracoForm<T>(
        this IHtmlHelper html,
        string action,
        object additionalRouteVals,
        object htmlAttributes,
        FormMethod method)
        where T : SurfaceController =>
        html.BeginUmbracoForm(action, typeof(T), additionalRouteVals, htmlAttributes, method);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm<T>(
        this IHtmlHelper html,
        string action,
        object additionalRouteVals,
        object htmlAttributes)
        where T : SurfaceController => html.BeginUmbracoForm(action, typeof(T), additionalRouteVals, htmlAttributes);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        Type surfaceType,
        object? additionalRouteVals,
        IDictionary<string, object?> htmlAttributes,
        FormMethod method)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(action));
        }

        if (surfaceType == null)
        {
            throw new ArgumentNullException(nameof(surfaceType));
        }

        SurfaceControllerTypeCollection surfaceControllerTypeCollection =
            GetRequiredService<SurfaceControllerTypeCollection>(html);
        Type? surfaceController = surfaceControllerTypeCollection.SingleOrDefault(x => x == surfaceType);
        if (surfaceController == null)
        {
            throw new InvalidOperationException("Could not find the surface controller of type " +
                                                surfaceType.FullName);
        }

        PluginControllerMetadata metaData = PluginController.GetMetadata(surfaceController);

        var area = string.Empty;
        if (metaData.AreaName.IsNullOrWhiteSpace() == false)
        {
            // Set the area to the plugin area
            area = metaData.AreaName;
        }

        return html.BeginUmbracoForm(
            action,
            metaData.ControllerName,
            area!,
            additionalRouteVals,
            htmlAttributes,
            method);
    }

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        Type surfaceType,
        object? additionalRouteVals,
        IDictionary<string, object?> htmlAttributes)
        => html.BeginUmbracoForm(action, surfaceType, additionalRouteVals, htmlAttributes, FormMethod.Post);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    /// <typeparam name="T">The <see cref="SurfaceController" /> type</typeparam>
    public static MvcForm BeginUmbracoForm<T>(
        this IHtmlHelper html,
        string action,
        object additionalRouteVals,
        IDictionary<string, object?> htmlAttributes,
        FormMethod method)
        where T : SurfaceController =>
        html.BeginUmbracoForm(action, typeof(T), additionalRouteVals, htmlAttributes, method);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    /// <typeparam name="T">The <see cref="SurfaceController" /> type</typeparam>
    public static MvcForm BeginUmbracoForm<T>(
        this IHtmlHelper html,
        string action,
        object additionalRouteVals,
        IDictionary<string, object?> htmlAttributes)
        where T : SurfaceController => html.BeginUmbracoForm(action, typeof(T), additionalRouteVals, htmlAttributes);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(this IHtmlHelper html, string action, string controllerName, string area, FormMethod method)
        => html.BeginUmbracoForm(action, controllerName, area, null, new Dictionary<string, object?>(), method);

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(this IHtmlHelper html, string action, string controllerName, string area)
        => html.BeginUmbracoForm(action, controllerName, area, null, new Dictionary<string, object?>());

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        string? controllerName,
        string area,
        object? additionalRouteVals,
        IDictionary<string, object?> htmlAttributes,
        FormMethod method)
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

        if (string.IsNullOrWhiteSpace(controllerName))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(controllerName));
        }

        IUmbracoContextAccessor umbracoContextAccessor = GetRequiredService<IUmbracoContextAccessor>(html);
        IUmbracoContext umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
        var formAction = umbracoContext.OriginalRequestUrl.PathAndQuery;
        return html.RenderForm(formAction, method, htmlAttributes, controllerName, action, area, additionalRouteVals);
    }

    /// <summary>
    ///     Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
    /// </summary>
    public static MvcForm BeginUmbracoForm(
        this IHtmlHelper html,
        string action,
        string controllerName,
        string area,
        object? additionalRouteVals,
        IDictionary<string, object?> htmlAttributes) =>
        html.BeginUmbracoForm(
            action,
            controllerName,
            area,
            additionalRouteVals,
            htmlAttributes,
            FormMethod.Post);

    /// <summary>
    ///     This renders out the form for us
    /// </summary>
    /// <remarks>
    ///     This code is pretty much the same as the underlying MVC code that writes out the form
    /// </remarks>
    private static MvcForm RenderForm(
        this IHtmlHelper htmlHelper,
        string formAction,
        FormMethod method,
        IDictionary<string, object?> htmlAttributes,
        string surfaceController,
        string surfaceAction,
        string area,
        object? additionalRouteVals = null)
    {
        // ensure that the multipart/form-data is added to the HTML attributes
        if (htmlAttributes.ContainsKey("enctype") == false)
        {
            htmlAttributes.Add("enctype", "multipart/form-data");
        }

        var tagBuilder = new TagBuilder("form");
        tagBuilder.MergeAttributes(htmlAttributes);

        // action is implicitly generated, so htmlAttributes take precedence.
        tagBuilder.MergeAttribute("action", formAction);

        // method is an explicit parameter, so it takes precedence over the htmlAttributes.
        tagBuilder.MergeAttribute("method", HtmlHelper.GetFormMethodString(method), true);
        var traditionalJavascriptEnabled = htmlHelper.ViewContext.ClientValidationEnabled;
        if (traditionalJavascriptEnabled)
        {
            // forms must have an ID for client validation
            tagBuilder.GenerateId("form" + Guid.NewGuid().ToString("N"), string.Empty);
        }

        htmlHelper.ViewContext.Writer.Write(tagBuilder.RenderStartTag());

        HtmlEncoder htmlEncoder = GetRequiredService<HtmlEncoder>(htmlHelper);

        // new UmbracoForm:
        var theForm = new UmbracoForm(htmlHelper.ViewContext, htmlEncoder, surfaceController, surfaceAction, area, additionalRouteVals);

        if (traditionalJavascriptEnabled)
        {
            htmlHelper.ViewContext.FormContext.FormData["FormId"] = tagBuilder.Attributes["id"];
        }

        return theForm;
    }

    /// <summary>
    ///     Used for rendering out the Form for BeginUmbracoForm
    /// </summary>
    internal class UmbracoForm : MvcForm
    {
        private readonly string _surfaceControllerInput;
        private readonly ViewContext _viewContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UmbracoForm" /> class.
        /// </summary>
        public UmbracoForm(
            ViewContext viewContext,
            HtmlEncoder htmlEncoder,
            string controllerName,
            string controllerAction,
            string area,
            object? additionalRouteVals = null)
            : base(viewContext, htmlEncoder)
        {
            _viewContext = viewContext;
            _surfaceControllerInput = GetSurfaceControllerHiddenInput(
                GetRequiredService<IDataProtectionProvider>(viewContext),
                controllerName,
                controllerAction,
                area,
                additionalRouteVals);
        }

        protected override void GenerateEndForm()
        {
            // Always output an anti-forgery token
            IAntiforgery antiforgery = _viewContext.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
            IHtmlContent antiforgeryHtml = antiforgery.GetHtml(_viewContext.HttpContext);
            _viewContext.Writer.Write(antiforgeryHtml.ToHtmlString());

            // write out the hidden surface form routes
            _viewContext.Writer.Write(_surfaceControllerInput);

            base.GenerateEndForm();
        }
    }

    #region If

    /// <summary>
    ///     If <paramref name="test" /> is <c>true</c>, the HTML encoded <paramref name="valueIfTrue" /> will be returned;
    ///     otherwise, <see cref="string.Empty" />.
    /// </summary>
    /// <param name="html">The HTML helper.</param>
    /// <param name="test">
    ///     If set to <c>true</c> returns <paramref name="valueIfTrue" />; otherwise,
    ///     <see cref="string.Empty" />.
    /// </param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    public static IHtmlContent If(this IHtmlHelper html, bool test, string valueIfTrue)
        => If(html, test, valueIfTrue, string.Empty);

    /// <summary>
    ///     If <paramref name="test" /> is <c>true</c>, the HTML encoded <paramref name="valueIfTrue" /> will be returned;
    ///     otherwise, <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="html">The HTML helper.</param>
    /// <param name="test">
    ///     If set to <c>true</c> returns <paramref name="valueIfTrue" />; otherwise,
    ///     <paramref name="valueIfFalse" />.
    /// </param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    public static IHtmlContent If(this IHtmlHelper html, bool test, string valueIfTrue, string valueIfFalse)
        => new HtmlString(HttpUtility.HtmlEncode(test ? valueIfTrue : valueIfFalse));

    #endregion

    #region Strings

    private static readonly HtmlStringUtilities s_stringUtilities = new();

    /// <summary>
    ///     HTML encodes the text and replaces text line breaks with HTML line breaks.
    /// </summary>
    /// <param name="helper">The HTML helper.</param>
    /// <param name="text">The text.</param>
    /// <returns>
    ///     The HTML encoded text with text line breaks replaced with HTML line breaks (<c>&lt;br /&gt;</c>).
    /// </returns>
    public static IHtmlContent ReplaceLineBreaks(this IHtmlHelper helper, string text)
        => s_stringUtilities.ReplaceLineBreaks(text);

    /// <summary>
    ///     Generates a hash based on the text string passed in.  This method will detect the
    ///     security requirements (is FIPS enabled) and return an appropriate hash.
    /// </summary>
    /// <param name="helper">The <see cref="IHtmlHelper" /></param>
    /// <param name="text">The text to create a hash from</param>
    /// <returns>Hash of the text string</returns>
    public static string CreateHash(this IHtmlHelper helper, string text) => text.GenerateHash();

    /// <summary>
    ///     Strips all HTML tags from a given string, all contents of the tags will remain.
    /// </summary>
    public static IHtmlContent StripHtml(this IHtmlHelper helper, IHtmlContent html, params string[] tags)
        => helper.StripHtml(html.ToHtmlString(), tags);

    /// <summary>
    ///     Strips all HTML tags from a given string, all contents of the tags will remain.
    /// </summary>
    public static IHtmlContent StripHtml(this IHtmlHelper helper, string html, params string[] tags)
        => s_stringUtilities.StripHtmlTags(html, tags);

    /// <summary>
    ///     Will take the first non-null value in the collection and return the value of it.
    /// </summary>
    public static string Coalesce(this IHtmlHelper helper, params object?[] args)
        => s_stringUtilities.Coalesce(args);

    /// <summary>
    ///     Joins any number of int/string/objects into one string
    /// </summary>
    public static string Concatenate(this IHtmlHelper helper, params object[] args)
        => s_stringUtilities.Concatenate(args);

    /// <summary>
    ///     Joins any number of int/string/objects into one string and separates them with the string separator parameter.
    /// </summary>
    public static string Join(this IHtmlHelper helper, string separator, params object[] args)
        => s_stringUtilities.Join(separator, args);

    /// <summary>
    ///     Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and
    ///     makes sure to close them
    /// </summary>
    public static IHtmlContent Truncate(this IHtmlHelper helper, IHtmlContent html, int length)
        => helper.Truncate(html.ToHtmlString(), length, true, false);

    /// <summary>
    ///     Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and
    ///     makes sure to close them
    /// </summary>
    public static IHtmlContent Truncate(this IHtmlHelper helper, IHtmlContent html, int length, bool addElipsis)
        => helper.Truncate(html.ToHtmlString(), length, addElipsis, false);

    /// <summary>
    ///     Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and
    ///     makes sure to close them
    /// </summary>
    public static IHtmlContent Truncate(this IHtmlHelper helper, IHtmlContent html, int length, bool addElipsis, bool treatTagsAsContent)
        => helper.Truncate(html.ToHtmlString(), length, addElipsis, treatTagsAsContent);

    /// <summary>
    ///     Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and
    ///     makes sure to close them
    /// </summary>
    public static IHtmlContent Truncate(this IHtmlHelper helper, string html, int length)
        => helper.Truncate(html, length, true, false);

    /// <summary>
    ///     Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and
    ///     makes sure to close them
    /// </summary>
    public static IHtmlContent Truncate(this IHtmlHelper helper, string html, int length, bool addElipsis)
        => helper.Truncate(html, length, addElipsis, false);

    /// <summary>
    ///     Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and
    ///     makes sure to close them
    /// </summary>
    public static IHtmlContent Truncate(this IHtmlHelper helper, string html, int length, bool addElipsis, bool treatTagsAsContent)
        => s_stringUtilities.Truncate(html, length, addElipsis, treatTagsAsContent);

    /// <summary>
    ///     Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML
    ///     tags, and makes sure to close them
    /// </summary>
    public static IHtmlContent TruncateByWords(this IHtmlHelper helper, string html, int words)
    {
        var length = s_stringUtilities.WordsToLength(html, words);

        return helper.Truncate(html, length, true, false);
    }

    /// <summary>
    ///     Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML
    ///     tags, and makes sure to close them
    /// </summary>
    public static IHtmlContent TruncateByWords(this IHtmlHelper helper, string html, int words, bool addElipsis)
    {
        var length = s_stringUtilities.WordsToLength(html, words);

        return helper.Truncate(html, length, addElipsis, false);
    }

    /// <summary>
    ///     Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML
    ///     tags, and makes sure to close them
    /// </summary>
    public static IHtmlContent TruncateByWords(this IHtmlHelper helper, IHtmlContent html, int words)
    {
        var length = s_stringUtilities.WordsToLength(html.ToHtmlString(), words);

        return helper.Truncate(html, length, true, false);
    }

    /// <summary>
    ///     Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML
    ///     tags, and makes sure to close them
    /// </summary>
    public static IHtmlContent TruncateByWords(this IHtmlHelper helper, IHtmlContent html, int words, bool addElipsis)
    {
        var length = s_stringUtilities.WordsToLength(html.ToHtmlString(), words);

        return helper.Truncate(html, length, addElipsis, false);
    }

    #endregion
}
