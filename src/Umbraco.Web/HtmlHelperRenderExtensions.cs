using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dynamics;
using Umbraco.Core.IO;
using Umbraco.Core.Profiling;
using Umbraco.Web.Mvc;
using umbraco;
using umbraco.cms.businesslogic.member;

namespace Umbraco.Web
{
    /// <summary>
	/// HtmlHelper extensions for use in templates
	/// </summary>
	public static class HtmlHelperRenderExtensions
	{
        /// <summary>
        /// Renders the markup for the profiler
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static IHtmlString RenderProfiler(this HtmlHelper helper)
        {
            return new HtmlString(ProfilerResolver.Current.Profiler.Render());
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
        public static MvcHtmlString AreaPartial(this HtmlHelper helper, string partial, string area, object model = null, ViewDataDictionary viewData = null)
        {
            var originalArea = helper.ViewContext.RouteData.DataTokens["area"];
            helper.ViewContext.RouteData.DataTokens["area"] = area;	        
            var result = helper.Partial(partial, model, viewData);
            helper.ViewContext.RouteData.DataTokens["area"] = originalArea;
            return result;
        }

	    /// <summary>
        /// Will render the preview badge when in preview mode which is not required ever unless the MVC page you are
        /// using does not inherit from UmbracoTemplatePage
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        /// <remarks>
        /// See: http://issues.umbraco.org/issue/U4-1614
        /// </remarks>
        public static MvcHtmlString PreviewBadge(this HtmlHelper helper)
        {
            if (UmbracoContext.Current.InPreviewMode)
            {
                var htmlBadge =
                    String.Format(UmbracoConfig.For.UmbracoSettings().Content.PreviewBadge,
                                  IOHelper.ResolveUrl(SystemDirectories.Umbraco),
                                  IOHelper.ResolveUrl(SystemDirectories.UmbracoClient),
                                  UmbracoContext.Current.HttpContext.Server.UrlEncode(UmbracoContext.Current.HttpContext.Request.Path));
                return new MvcHtmlString(htmlBadge);
            }
            return new MvcHtmlString("");

        }

		public static IHtmlString CachedPartial(
			this HtmlHelper htmlHelper, 
			string partialViewName, 
			object model, 
			int cachedSeconds,
			bool cacheByPage = false,
			bool cacheByMember = false,
			ViewDataDictionary viewData = null,
			Func<object, ViewDataDictionary, string> contextualKeyBuilder = null)
		{
			var cacheKey = new StringBuilder(partialViewName);
			if (cacheByPage)
			{
				if (UmbracoContext.Current == null)
				{
					throw new InvalidOperationException("Cannot cache by page if the UmbracoContext has not been initialized, this parameter can only be used in the context of an Umbraco request");
				}
				cacheKey.AppendFormat("{0}-", UmbracoContext.Current.PageId);
			}
			if (cacheByMember)
			{
				var currentMember = Member.GetCurrentMember();
				cacheKey.AppendFormat("m{0}-", currentMember == null ? 0 : currentMember.Id);
			}
			if (contextualKeyBuilder != null)
		    {
		        var contextualKey = contextualKeyBuilder(model, viewData);
                cacheKey.AppendFormat("c{0}-", contextualKey);
		    }
			return ApplicationContext.Current.ApplicationCache.CachedPartialView(htmlHelper, partialViewName, model, cachedSeconds, cacheKey.ToString(), viewData);
		}

		public static MvcHtmlString EditorFor<T>(this HtmlHelper htmlHelper, string templateName = "", string htmlFieldName = "", object additionalViewData = null)
			where T : new()
		{
			var model = new T();
			var typedHelper = new HtmlHelper<T>(
				htmlHelper.ViewContext.CopyWithModel(model),
				htmlHelper.ViewDataContainer.CopyWithModel(model));

			return typedHelper.EditorFor(x => model, templateName, htmlFieldName, additionalViewData);
		}

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
		public static MvcHtmlString ValidationSummary(this HtmlHelper htmlHelper,
		                                              string prefix = "",
		                                              bool excludePropertyErrors = false,
		                                              string message = "",
		                                              IDictionary<string, object> htmlAttributes = null)
		{
			if (prefix.IsNullOrWhiteSpace())
			{
				return htmlHelper.ValidationSummary(excludePropertyErrors, message, htmlAttributes);
			}

			//if there's a prefix applied, we need to create a new html helper with a filtered ModelState collection so that it only looks for 
			//specific model state with the prefix.
			var filteredHtmlHelper = new HtmlHelper(htmlHelper.ViewContext, htmlHelper.ViewDataContainer.FilterContainer(prefix));
			return filteredHtmlHelper.ValidationSummary(excludePropertyErrors, message, htmlAttributes);
		}

	    /// <summary>
	    /// Returns the result of a child action of a strongly typed SurfaceController
	    /// </summary>
	    /// <typeparam name="T"></typeparam>
	    /// <param name="htmlHelper"></param>
	    /// <param name="actionName"></param>
	    /// <returns></returns>
	    public static IHtmlString Action<T>(this HtmlHelper htmlHelper, string actionName)
            where T : SurfaceController
        {
            return htmlHelper.Action(actionName, typeof(T));
        }

        /// <summary>
        /// Returns the result of a child action of a SurfaceController
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="actionName"></param>
        /// <param name="surfaceType"></param>
        /// <returns></returns>
        public static IHtmlString Action(this HtmlHelper htmlHelper, string actionName, Type surfaceType)
        {
            Mandate.ParameterNotNull(surfaceType, "surfaceType");
            Mandate.ParameterNotNullOrEmpty(actionName, "actionName");

            var routeVals = new RouteValueDictionary(new {area = ""});

            var surfaceController = SurfaceControllerResolver.Current.RegisteredSurfaceControllers
                .SingleOrDefault(x => x == surfaceType);
            if (surfaceController == null)
                throw new InvalidOperationException("Could not find the surface controller of type " + surfaceType.FullName);
            var metaData = PluginController.GetMetadata(surfaceController);
            if (!metaData.AreaName.IsNullOrWhiteSpace())
            {
                //set the area to the plugin area
                if (routeVals.ContainsKey("area"))
                {
                    routeVals["area"] = metaData.AreaName;
                }
                else
                {
                    routeVals.Add("area", metaData.AreaName);    
                }
            }

            return htmlHelper.Action(actionName, metaData.ControllerName, routeVals);
        }

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
		    /// <param name="controllerName"></param>
		    /// <param name="controllerAction"></param>
		    /// <param name="area"></param>
		    /// <param name="method"></param>
		    /// <param name="additionalRouteVals"></param>
		    public UmbracoForm(
				ViewContext viewContext,
				string controllerName,
				string controllerAction,
				string area,
                FormMethod method,
				object additionalRouteVals = null)
				: base(viewContext)
			{
		        _viewContext = viewContext;
		        _method = method;
                _encryptedString = UmbracoHelper.CreateEncryptedRouteString(controllerName, controllerAction, area, additionalRouteVals);
			}

		    private readonly ViewContext _viewContext;
		    private readonly FormMethod _method;
			private bool _disposed;
			private readonly string _encryptedString;

			protected override void Dispose(bool disposing)
			{
				if (this._disposed)
					return;
				this._disposed = true;

                //write out the hidden surface form routes
                _viewContext.Writer.Write("<input name='ufprt' type='hidden' value='" + _encryptedString + "' />");

				base.Dispose(disposing);
			}
		}

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="method"></param>
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
            Mandate.ParameterNotNullOrEmpty(action, "action");
            Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");

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
			Mandate.ParameterNotNullOrEmpty(action, "action");
			Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");

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
            Mandate.ParameterNotNullOrEmpty(action, "action");
            Mandate.ParameterNotNull(surfaceType, "surfaceType");

            var area = "";

            var surfaceController = SurfaceControllerResolver.Current.RegisteredSurfaceControllers
                                                             .SingleOrDefault(x => x == surfaceType);
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
            Mandate.ParameterNotNullOrEmpty(action, "action");
            Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");

            var formAction = UmbracoContext.Current.OriginalRequestUrl.PathAndQuery;
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

			//ensure that the multipart/form-data is added to the html attributes
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
			var traditionalJavascriptEnabled = htmlHelper.ViewContext.ClientValidationEnabled && htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled == false;
			if (traditionalJavascriptEnabled)
			{
				// forms must have an ID for client validation
				tagBuilder.GenerateId("form" + Guid.NewGuid().ToString("N"));
			}
			htmlHelper.ViewContext.Writer.Write(tagBuilder.ToString(TagRenderMode.StartTag));

			//new UmbracoForm:
			var theForm = new UmbracoForm(htmlHelper.ViewContext, surfaceController, surfaceAction, area, method, additionalRouteVals);

			if (traditionalJavascriptEnabled)
			{
				htmlHelper.ViewContext.FormContext.FormId = tagBuilder.Attributes["id"];
			}
			return theForm;
		}

		#endregion


		#region Wrap

		public static HtmlTagWrapper Wrap(this HtmlHelper html, string tag, string innerText, params IHtmlTagWrapper[] children)
		{
			var item = html.Wrap(tag, innerText, (object)null);
			foreach (var child in children)
			{
				item.AddChild(child);
			}
			return item;
		}

		public static HtmlTagWrapper Wrap(this HtmlHelper html, string tag, object inner, object anonymousAttributes, params IHtmlTagWrapper[] children)
		{
			string innerText = null;
			if (inner != null && inner.GetType() != typeof(DynamicNull))
			{
				innerText = string.Format("{0}", inner);
			}
			var item = html.Wrap(tag, innerText, anonymousAttributes);
			foreach (var child in children)
			{
				item.AddChild(child);
			}
			return item;
		}
		public static HtmlTagWrapper Wrap(this HtmlHelper html, string tag, object inner)
		{
			string innerText = null;
			if (inner != null && inner.GetType() != typeof(DynamicNull))
			{
				innerText = string.Format("{0}", inner);
			}
			return html.Wrap(tag, innerText, (object)null);
		}

		public static HtmlTagWrapper Wrap(this HtmlHelper html, string tag, string innerText, object anonymousAttributes, params IHtmlTagWrapper[] children)
		{
			var wrap = new HtmlTagWrapper(tag);
			if (anonymousAttributes != null)
			{
				wrap.ReflectAttributesFromAnonymousType(anonymousAttributes);
			}
			if (!string.IsNullOrWhiteSpace(innerText))
			{
				wrap.AddChild(new HtmlTagWrapperTextNode(innerText));
			}
			foreach (var child in children)
			{
				wrap.AddChild(child);
			}
			return wrap;
		}

		public static HtmlTagWrapper Wrap(this HtmlHelper html, bool visible, string tag, string innerText, object anonymousAttributes, params IHtmlTagWrapper[] children)
		{
			var item = html.Wrap(tag, innerText, anonymousAttributes, children);
			item.Visible = visible;
			foreach (var child in children)
			{
				item.AddChild(child);
			}
			return item;
		}

		#endregion

	}
}
