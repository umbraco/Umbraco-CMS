using System;
using System.Collections.Generic;
using System.Linq;
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
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Mvc;
using Umbraco.Web.Common.Security;
using Umbraco.Web.Website.Collections;
using Umbraco.Web.Website.Controllers;

namespace Umbraco.Extensions
{
    /// <summary>
    /// HtmlHelper extensions for use in templates
    /// </summary>
    public static class HtmlHelperRenderExtensions
    {
       private static T GetRequiredService<T>(IHtmlHelper htmlHelper)
       {
           return GetRequiredService<T>(htmlHelper.ViewContext);
       }

        private static T GetRequiredService<T>(ViewContext viewContext)
       {
           return viewContext.HttpContext.RequestServices.GetRequiredService<T>();
       }

        /// <summary>
        /// Renders the markup for the profiler
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static IHtmlContent RenderProfiler(this IHtmlHelper helper)
        {
            return new HtmlString(GetRequiredService<IProfilerHtml>(helper).Render());
        }

        /// <summary>
        /// Renders a partial view that is found in the specified area
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="partial"></param>
        /// <param name="area"></param>
        /// <param name="model"></param>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public static IHtmlContent AreaPartial(this IHtmlHelper helper, string partial, string area, object model = null, ViewDataDictionary viewData = null)
        {
            var originalArea = helper.ViewContext.RouteData.DataTokens["area"];
            helper.ViewContext.RouteData.DataTokens["area"] = area;
            var result = helper.Partial(partial, model, viewData);
            helper.ViewContext.RouteData.DataTokens["area"] = originalArea;
            return result;
        }

        /// <summary>
        /// Will render the preview badge when in preview mode which is not required ever unless the MVC page you are
        /// using does not inherit from UmbracoViewPage
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        /// <remarks>
        /// See: http://issues.umbraco.org/issue/U4-1614
        /// </remarks>
        public static IHtmlContent PreviewBadge(this IHtmlHelper helper, IUmbracoContextAccessor umbracoContextAccessor, IHttpContextAccessor httpContextAccessor, GlobalSettings globalSettings, IIOHelper ioHelper, ContentSettings contentSettings)
        {
            var umbrcoContext = umbracoContextAccessor.UmbracoContext;
            if (umbrcoContext.InPreviewMode)
            {
                var htmlBadge =
                    String.Format(contentSettings.PreviewBadge,
                                ioHelper.ResolveUrl(globalSettings.UmbracoPath),
                                  WebUtility.UrlEncode(httpContextAccessor.GetRequiredHttpContext().Request.Path),
                                umbrcoContext.PublishedRequest.PublishedContent.Id);
                return new HtmlString(htmlBadge);
            }
            return new HtmlString("");

        }

        public static IHtmlContent CachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            int cachedSeconds,
            bool cacheByPage = false,
            bool cacheByMember = false,
            ViewDataDictionary viewData = null,
            Func<object, ViewDataDictionary, string> contextualKeyBuilder = null)
        {
            var cacheKey = new StringBuilder(partialViewName);
             //let's always cache by the current culture to allow variants to have different cache results
            var cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            if (!String.IsNullOrEmpty(cultureName))
            {
                cacheKey.AppendFormat("{0}-", cultureName);
            }
            if (cacheByPage)
            {
                var umbracoContextAccessor = GetRequiredService<IUmbracoContextAccessor>(htmlHelper);
                var umbracoContext = umbracoContextAccessor.UmbracoContext;
                if (umbracoContext == null)
                {
                    throw new InvalidOperationException("Cannot cache by page if the UmbracoContext has not been initialized, this parameter can only be used in the context of an Umbraco request");
                }
                cacheKey.AppendFormat("{0}-", umbracoContext.PublishedRequest?.PublishedContent?.Id ?? 0);
            }
            if (cacheByMember)
            {
                //TODO reintroduce when members are migrated
                throw new NotImplementedException("Reintroduce when members are migrated");
                    // var helper = Current.MembershipHelper;
                // var currentMember = helper.GetCurrentMember();
                // cacheKey.AppendFormat("m{0}-", currentMember?.Id ?? 0);
            }
            if (contextualKeyBuilder != null)
            {
                var contextualKey = contextualKeyBuilder(model, viewData);
                cacheKey.AppendFormat("c{0}-", contextualKey);
            }

            var appCaches = GetRequiredService<AppCaches>(htmlHelper);
            var hostingEnvironment = GetRequiredService<IHostingEnvironment>(htmlHelper);

            return appCaches.CachedPartialView(hostingEnvironment, htmlHelper, partialViewName, model, cachedSeconds, cacheKey.ToString(), viewData);
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
        /// A validation summary that lets you pass in a prefix so that the summary only displays for elements
        /// containing the prefix. This allows you to have more than on validation summary on a page.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="prefix"></param>
        /// <param name="excludePropertyErrors"></param>
        /// <param name="message"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static IHtmlContent ValidationSummary(this IHtmlHelper htmlHelper,
                                                     string prefix = "",
                                                     bool excludePropertyErrors = false,
                                                     string message = "",
                                                     object htmlAttributes = null)
        {
            if (prefix.IsNullOrWhiteSpace())
            {
                return htmlHelper.ValidationSummary(excludePropertyErrors, message, htmlAttributes);
            }




            var htmlGenerator = GetRequiredService<IHtmlGenerator>(htmlHelper);

            var viewContext = htmlHelper.ViewContext.Clone();
            foreach (var key in viewContext.ViewData.Keys.ToArray()){
                 if(!key.StartsWith(prefix)){
                    viewContext.ViewData.Remove(key);
                 }
             }
            var tagBuilder = htmlGenerator.GenerateValidationSummary(
                viewContext,
                excludePropertyErrors,
                message,
                headerTag: null,
                htmlAttributes: htmlAttributes);
            if (tagBuilder == null)
            {
                return HtmlString.Empty;
            }

            return tagBuilder;
        }

// TODO what to do here? This will be view components right?
        // /// <summary>
        // /// Returns the result of a child action of a strongly typed SurfaceController
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <param name="htmlHelper"></param>
        // /// <param name="actionName"></param>
        // /// <returns></returns>
        // public static IHtmlContent Action<T>(this HtmlHelper htmlHelper, string actionName)
        //     where T : SurfaceController
        // {
        //     return htmlHelper.Action(actionName, typeof(T));
        // }
        //

// TODO what to do here? This will be view components right?
        // /// <summary>
        // /// Returns the result of a child action of a SurfaceController
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <param name="htmlHelper"></param>
        // /// <param name="actionName"></param>
        // /// <param name="surfaceType"></param>
        // /// <returns></returns>
        // public static IHtmlContent Action(this IHtmlHelper htmlHelper, string actionName, Type surfaceType)
        // {
        //     if (actionName == null) throw new ArgumentNullException(nameof(actionName));
        //     if (string.IsNullOrWhiteSpace(actionName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(actionName));
        //     if (surfaceType == null) throw new ArgumentNullException(nameof(surfaceType));
        //
        //     var routeVals = new RouteValueDictionary(new {area = ""});
        //
        //     var surfaceControllerTypeCollection = GetRequiredService<SurfaceControllerTypeCollection>(htmlHelper);
        //     var surfaceController = surfaceControllerTypeCollection.SingleOrDefault(x => x == surfaceType);
        //     if (surfaceController == null)
        //         throw new InvalidOperationException("Could not find the surface controller of type " + surfaceType.FullName);
        //     var metaData = PluginController.GetMetadata(surfaceController);
        //     if (!metaData.AreaName.IsNullOrWhiteSpace())
        //     {
        //         //set the area to the plugin area
        //         if (routeVals.ContainsKey("area"))
        //         {
        //             routeVals["area"] = metaData.AreaName;
        //         }
        //         else
        //         {
        //             routeVals.Add("area", metaData.AreaName);
        //         }
        //     }
        //
        //     return htmlHelper.Action(actionName, metaData.ControllerName, routeVals);
        // }

        #region BeginUmbracoForm

        /// <summary>
        /// Used for rendering out the Form for BeginUmbracoForm
        /// </summary>
        internal class UmbracoForm : MvcForm
        {
            /// <summary>
            /// Creates an UmbracoForm
            /// </summary>
            /// <param name="viewContext"></param>
            /// <param name="htmlEncoder"></param>
            /// <param name="controllerName"></param>
            /// <param name="controllerAction"></param>
            /// <param name="area"></param>
            /// <param name="method"></param>
            /// <param name="additionalRouteVals"></param>
            public UmbracoForm(
                ViewContext viewContext,
                HtmlEncoder htmlEncoder,
                string controllerName,
                string controllerAction,
                string area,
                FormMethod method,
                object additionalRouteVals = null)
                : base(viewContext, htmlEncoder)
            {
                _viewContext = viewContext;
                _method = method;
                _controllerName = controllerName;
                _encryptedString = EncryptionHelper.CreateEncryptedRouteString(GetRequiredService<IDataProtectionProvider>(viewContext), controllerName, controllerAction, area, additionalRouteVals);
            }


            private readonly ViewContext _viewContext;
            private readonly FormMethod _method;
            private bool _disposed;
            private readonly string _encryptedString;
            private readonly string _controllerName;

            protected new void Dispose()
            {
                if (this._disposed)
                    return;
                this._disposed = true;
                //Detect if the call is targeting UmbRegisterController/UmbProfileController/UmbLoginStatusController/UmbLoginController and if it is we automatically output a AntiForgeryToken()
                // We have a controllerName and area so we can match
                if (_controllerName == "UmbRegister"
                    || _controllerName == "UmbProfile"
                    || _controllerName == "UmbLoginStatus"
                    || _controllerName == "UmbLogin")
                {
                     var antiforgery = _viewContext.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
                    _viewContext.Writer.Write(antiforgery.GetHtml(_viewContext.HttpContext).ToString());
                }

                //write out the hidden surface form routes
                _viewContext.Writer.Write("<input name=\"ufprt\" type=\"hidden\" value=\"" + _encryptedString + "\" />");

                base.Dispose();
            }
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller.
        /// </summary>
        /// <param name="html">The HTML helper.</param>
        /// <param name="action">Name of the action.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName, FormMethod method)
        {
            return html.BeginUmbracoForm(action, controllerName, null, new Dictionary<string, object>(), method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName)
        {
            return html.BeginUmbracoForm(action, controllerName, null, new Dictionary<string, object>());
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName, object additionalRouteVals, FormMethod method)
        {
            return html.BeginUmbracoForm(action, controllerName, additionalRouteVals, new Dictionary<string, object>(), method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName, object additionalRouteVals)
        {
            return html.BeginUmbracoForm(action, controllerName, additionalRouteVals, new Dictionary<string, object>());
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName,
            object additionalRouteVals,
            object htmlAttributes,
            FormMethod method)
        {
            return html.BeginUmbracoForm(action, controllerName, additionalRouteVals, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes), method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName,
            object additionalRouteVals,
            object htmlAttributes)
        {
            return html.BeginUmbracoForm(action, controllerName, additionalRouteVals, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName,
            object additionalRouteVals,
            IDictionary<string, object> htmlAttributes,
            FormMethod method)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(action));
            if (controllerName == null) throw new ArgumentNullException(nameof(controllerName));
            if (string.IsNullOrWhiteSpace(controllerName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(controllerName));

            return html.BeginUmbracoForm(action, controllerName, "", additionalRouteVals, htmlAttributes, method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName,
            object additionalRouteVals,
            IDictionary<string, object> htmlAttributes)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(action));
            if (controllerName == null) throw new ArgumentNullException(nameof(controllerName));
            if (string.IsNullOrWhiteSpace(controllerName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(controllerName));

            return html.BeginUmbracoForm(action, controllerName, "", additionalRouteVals, htmlAttributes);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceType">The surface controller to route to</param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Type surfaceType, FormMethod method)
        {
            return html.BeginUmbracoForm(action, surfaceType, null, new Dictionary<string, object>(), method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceType">The surface controller to route to</param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Type surfaceType)
        {
            return html.BeginUmbracoForm(action, surfaceType, null, new Dictionary<string, object>());
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm<T>(this HtmlHelper html, string action, FormMethod method)
            where T : SurfaceController
        {
            return html.BeginUmbracoForm(action, typeof(T), method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm<T>(this HtmlHelper html, string action)
            where T : SurfaceController
        {
            return html.BeginUmbracoForm(action, typeof(T));
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceType">The surface controller to route to</param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Type surfaceType,
                                               object additionalRouteVals, FormMethod method)
        {
            return html.BeginUmbracoForm(action, surfaceType, additionalRouteVals, new Dictionary<string, object>(), method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceType">The surface controller to route to</param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Type surfaceType,
                                               object additionalRouteVals)
        {
            return html.BeginUmbracoForm(action, surfaceType, additionalRouteVals, new Dictionary<string, object>());
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm<T>(this HtmlHelper html, string action, object additionalRouteVals, FormMethod method)
            where T : SurfaceController
        {
            return html.BeginUmbracoForm(action, typeof(T), additionalRouteVals, method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm<T>(this HtmlHelper html, string action, object additionalRouteVals)
            where T : SurfaceController
        {
            return html.BeginUmbracoForm(action, typeof(T), additionalRouteVals);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceType">The surface controller to route to</param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Type surfaceType,
                                               object additionalRouteVals,
                                               object htmlAttributes,
                                               FormMethod method)
        {
            return html.BeginUmbracoForm(action, surfaceType, additionalRouteVals, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes), method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceType">The surface controller to route to</param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Type surfaceType,
                                               object additionalRouteVals,
                                               object htmlAttributes)
        {
            return html.BeginUmbracoForm(action, surfaceType, additionalRouteVals, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm<T>(this HtmlHelper html, string action,
                                                  object additionalRouteVals,
                                                  object htmlAttributes,
                                                  FormMethod method)
            where T : SurfaceController
        {
            return html.BeginUmbracoForm(action, typeof(T), additionalRouteVals, htmlAttributes, method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm<T>(this HtmlHelper html, string action,
                                               object additionalRouteVals,
                                               object htmlAttributes)
            where T : SurfaceController
        {
            return html.BeginUmbracoForm(action, typeof(T), additionalRouteVals, htmlAttributes);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceType">The surface controller to route to</param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Type surfaceType,
                                               object additionalRouteVals,
                                               IDictionary<string, object> htmlAttributes,
                                               FormMethod method)
        {

            if (action == null) throw new ArgumentNullException(nameof(action));
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(action));
            if (surfaceType == null) throw new ArgumentNullException(nameof(surfaceType));

            var area = "";

            var surfaceControllerTypeCollection = GetRequiredService<SurfaceControllerTypeCollection>(html);
            var surfaceController = surfaceControllerTypeCollection.SingleOrDefault(x => x == surfaceType);
            if (surfaceController == null)
                throw new InvalidOperationException("Could not find the surface controller of type " + surfaceType.FullName);
            var metaData = PluginController.GetMetadata(surfaceController);
            if (metaData.AreaName.IsNullOrWhiteSpace() == false)
            {
                //set the area to the plugin area
                area = metaData.AreaName;
            }
            return html.BeginUmbracoForm(action, metaData.ControllerName, area, additionalRouteVals, htmlAttributes, method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceType">The surface controller to route to</param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Type surfaceType,
                                               object additionalRouteVals,
                                               IDictionary<string, object> htmlAttributes)
        {
            return html.BeginUmbracoForm(action, surfaceType, additionalRouteVals, htmlAttributes, FormMethod.Post);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm<T>(this HtmlHelper html, string action,
                                                  object additionalRouteVals,
                                                  IDictionary<string, object> htmlAttributes,
                                                  FormMethod method)
            where T : SurfaceController
        {
            return html.BeginUmbracoForm(action, typeof(T), additionalRouteVals, htmlAttributes, method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm<T>(this HtmlHelper html, string action,
                                               object additionalRouteVals,
                                               IDictionary<string, object> htmlAttributes)
            where T : SurfaceController
        {
            return html.BeginUmbracoForm(action, typeof(T), additionalRouteVals, htmlAttributes);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="area"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName, string area, FormMethod method)
        {
            return html.BeginUmbracoForm(action, controllerName, area, null, new Dictionary<string, object>(), method);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName, string area)
        {
            return html.BeginUmbracoForm(action, controllerName, area, null, new Dictionary<string, object>());
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="area"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName, string area,
                                               object additionalRouteVals,
                                               IDictionary<string, object> htmlAttributes,
                                               FormMethod method)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (string.IsNullOrEmpty(action)) throw new ArgumentException("Value can't be empty.", nameof(action));
            if (controllerName == null) throw new ArgumentNullException(nameof(controllerName));
            if (string.IsNullOrWhiteSpace(controllerName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(controllerName));

            var umbracoContextAccessor = GetRequiredService<IUmbracoContextAccessor>(html);
            var formAction = umbracoContextAccessor.UmbracoContext.OriginalRequestUrl.PathAndQuery;
            return html.RenderForm(formAction, method, htmlAttributes, controllerName, action, area, additionalRouteVals);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="area"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName, string area,
                                               object additionalRouteVals,
                                               IDictionary<string, object> htmlAttributes)
        {
            return html.BeginUmbracoForm(action, controllerName, area, additionalRouteVals, htmlAttributes, FormMethod.Post);
        }

        /// <summary>
        /// This renders out the form for us
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="formAction"></param>
        /// <param name="method"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="surfaceController"></param>
        /// <param name="surfaceAction"></param>
        /// <param name="area"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        /// <remarks>
        /// This code is pretty much the same as the underlying MVC code that writes out the form
        /// </remarks>
        private static MvcForm RenderForm(this HtmlHelper htmlHelper,
                                          string formAction,
                                          FormMethod method,
                                          IDictionary<string, object> htmlAttributes,
                                          string surfaceController,
                                          string surfaceAction,
                                          string area,
                                          object additionalRouteVals = null)
        {

            //ensure that the multipart/form-data is added to the HTML attributes
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
                tagBuilder.GenerateId("form" + Guid.NewGuid().ToString("N"),  string.Empty);
            }

            htmlHelper.ViewContext.Writer.Write(tagBuilder.RenderStartTag());

            var htmlEncoder = GetRequiredService<HtmlEncoder>(htmlHelper);
            //new UmbracoForm:
            var theForm = new UmbracoForm(htmlHelper.ViewContext,  htmlEncoder, surfaceController, surfaceAction, area, method, additionalRouteVals);

            if (traditionalJavascriptEnabled)
            {
                htmlHelper.ViewContext.FormContext.FormData["FormId"] = tagBuilder.Attributes["id"];
            }
            return theForm;
        }

        #endregion

        // #region Wrap
        //
        // public static HtmlTagWrapper Wrap(this HtmlHelper html, string tag, string innerText, params IHtmlTagWrapper[] children)
        // {
        //     var item = html.Wrap(tag, innerText, (object)null);
        //     foreach (var child in children)
        //     {
        //         item.AddChild(child);
        //     }
        //     return item;
        // }
        //
        // public static HtmlTagWrapper Wrap(this HtmlHelper html, string tag, object inner, object anonymousAttributes, params IHtmlTagWrapper[] children)
        // {
        //     string innerText = null;
        //     if (inner != null)
        //     {
        //         innerText = string.Format("{0}", inner);
        //     }
        //     var item = html.Wrap(tag, innerText, anonymousAttributes);
        //     foreach (var child in children)
        //     {
        //         item.AddChild(child);
        //     }
        //     return item;
        // }
        // public static HtmlTagWrapper Wrap(this HtmlHelper html, string tag, object inner)
        // {
        //     string innerText = null;
        //     if (inner != null)
        //     {
        //         innerText = string.Format("{0}", inner);
        //     }
        //     return html.Wrap(tag, innerText, (object)null);
        // }
        //
        // public static HtmlTagWrapper Wrap(this HtmlHelper html, string tag, string innerText, object anonymousAttributes, params IHtmlTagWrapper[] children)
        // {
        //     var wrap = new HtmlTagWrapper(tag);
        //     if (anonymousAttributes != null)
        //     {
        //         wrap.ReflectAttributesFromAnonymousType(anonymousAttributes);
        //     }
        //     if (!string.IsNullOrWhiteSpace(innerText))
        //     {
        //         wrap.AddChild(new HtmlTagWrapperTextNode(innerText));
        //     }
        //     foreach (var child in children)
        //     {
        //         wrap.AddChild(child);
        //     }
        //     return wrap;
        // }
        //
        // public static HtmlTagWrapper Wrap(this HtmlHelper html, bool visible, string tag, string innerText, object anonymousAttributes, params IHtmlTagWrapper[] children)
        // {
        //     var item = html.Wrap(tag, innerText, anonymousAttributes, children);
        //     item.Visible = visible;
        //     return item;
        // }
        //
        // #endregion

        #region If

        /// <summary>
        /// If <paramref name="test" /> is <c>true</c>, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="html">The HTML helper.</param>
        /// <param name="test">If set to <c>true</c> returns <paramref name="valueIfTrue" />; otherwise, <see cref="string.Empty" />.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        public static IHtmlContent If(this HtmlHelper html, bool test, string valueIfTrue)
        {
            return If(html, test, valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If <paramref name="test" /> is <c>true</c>, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="html">The HTML helper.</param>
        /// <param name="test">If set to <c>true</c> returns <paramref name="valueIfTrue" />; otherwise, <paramref name="valueIfFalse" />.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        public static IHtmlContent If(this HtmlHelper html, bool test, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(test ? valueIfTrue : valueIfFalse));
        }

        #endregion

        #region Strings

        private static readonly HtmlStringUtilities StringUtilities = new HtmlStringUtilities();

        /// <summary>
        /// HTML encodes the text and replaces text line breaks with HTML line breaks.
        /// </summary>
        /// <param name="helper">The HTML helper.</param>
        /// <param name="text">The text.</param>
        /// <returns>
        /// The HTML encoded text with text line breaks replaced with HTML line breaks (<c>&lt;br /&gt;</c>).
        /// </returns>
        public static IHtmlContent ReplaceLineBreaks(this HtmlHelper helper, string text)
        {
            return StringUtilities.ReplaceLineBreaks(text);
        }

        /// <summary>
        /// Generates a hash based on the text string passed in.  This method will detect the
        /// security requirements (is FIPS enabled) and return an appropriate hash.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="text">The text to create a hash from</param>
        /// <returns>Hash of the text string</returns>
        public static string CreateHash(this HtmlHelper helper, string text)
        {
            return text.GenerateHash();
        }

        /// <summary>
        /// Strips all HTML tags from a given string, all contents of the tags will remain.
        /// </summary>
        public static IHtmlContent StripHtml(this HtmlHelper helper, IHtmlContent html, params string[] tags)
        {
            return helper.StripHtml(html.ToHtmlString(), tags);
        }

        public static string ToHtmlString(this IHtmlContent content)
        {
            using (var writer = new System.IO.StringWriter())
            {
                content.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Strips all HTML tags from a given string, all contents of the tags will remain.
        /// </summary>
        public static IHtmlContent StripHtml(this HtmlHelper helper, string html, params string[] tags)
        {
            return StringUtilities.StripHtmlTags(html, tags);
        }

        /// <summary>
        /// Will take the first non-null value in the collection and return the value of it.
        /// </summary>
        public static string Coalesce(this HtmlHelper helper, params object[] args)
        {
            return StringUtilities.Coalesce(args);
        }

        /// <summary>
        /// Joins any number of int/string/objects into one string
        /// </summary>
        public static string Concatenate(this HtmlHelper helper, params object[] args)
        {
            return StringUtilities.Concatenate(args);
        }

        /// <summary>
        /// Joins any number of int/string/objects into one string and separates them with the string separator parameter.
        /// </summary>
        public static string Join(this HtmlHelper helper, string separator, params object[] args)
        {
            return StringUtilities.Join(separator, args);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public static IHtmlContent Truncate(this HtmlHelper helper, IHtmlContent html, int length)
        {
            return helper.Truncate(html.ToHtmlString(), length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public static IHtmlContent Truncate(this HtmlHelper helper, IHtmlContent html, int length, bool addElipsis)
        {
            return helper.Truncate(html.ToHtmlString(), length, addElipsis, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public static IHtmlContent Truncate(this HtmlHelper helper, IHtmlContent html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            return helper.Truncate(html.ToHtmlString(), length, addElipsis, treatTagsAsContent);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public static IHtmlContent Truncate(this HtmlHelper helper, string html, int length)
        {
            return helper.Truncate(html, length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public static IHtmlContent Truncate(this HtmlHelper helper, string html, int length, bool addElipsis)
        {
            return helper.Truncate(html, length, addElipsis, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public static IHtmlContent Truncate(this HtmlHelper helper, string html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            return StringUtilities.Truncate(html, length, addElipsis, treatTagsAsContent);
        }

        #region Truncate by Words

        /// <summary>
        /// Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public static IHtmlContent TruncateByWords(this HtmlHelper helper, string html, int words)
        {
            int length = StringUtilities.WordsToLength(html, words);

            return helper.Truncate(html, length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public static IHtmlContent TruncateByWords(this HtmlHelper helper, string html, int words, bool addElipsis)
        {
            int length = StringUtilities.WordsToLength(html, words);

            return helper.Truncate(html, length, addElipsis, false);
        }

        /// <summary>
        /// Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public static IHtmlContent TruncateByWords(this HtmlHelper helper, IHtmlContent html, int words)
        {
            int length = StringUtilities.WordsToLength(html.ToHtmlString(), words);

            return helper.Truncate(html, length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public static IHtmlContent TruncateByWords(this HtmlHelper helper, IHtmlContent html, int words, bool addElipsis)
        {
            int length = StringUtilities.WordsToLength(html.ToHtmlString(), words);

            return helper.Truncate(html, length, addElipsis, false);
        }

        #endregion

        #endregion
    }
}
