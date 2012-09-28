using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Web.Mvc;
using umbraco;

namespace Umbraco.Web
{
	/// <summary>
	/// HtmlHelper extensions for use in templates
	/// </summary>
	public static class HtmlHelperRenderExtensions
	{

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
			/// <param name="surfaceController"></param>
			/// <param name="surfaceAction"></param>
			/// <param name="area"></param>		
			/// <param name="additionalRouteVals"></param>
			public UmbracoForm(
				ViewContext viewContext,
				string surfaceController,
				string surfaceAction,
				string area,
				object additionalRouteVals = null)
				: base(viewContext)
			{
				//need to create a params string as Base64 to put into our hidden field to use during the routes
				var surfaceRouteParams = string.Format("c={0}&a={1}&ar={2}",
														  viewContext.HttpContext.Server.UrlEncode(surfaceController),
														  viewContext.HttpContext.Server.UrlEncode(surfaceAction),
														  area);

				var additionalRouteValsAsQuery = additionalRouteVals.ToDictionary<object>().ToQueryString();
				if (!additionalRouteValsAsQuery.IsNullOrWhiteSpace())
					surfaceRouteParams = "&" + additionalRouteValsAsQuery;

				_base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(surfaceRouteParams));

				_textWriter = viewContext.Writer;
			}


			private bool _disposed;
			private readonly string _base64String;
			private readonly TextWriter _textWriter;

			protected override void Dispose(bool disposing)
			{
				if (this._disposed)
					return;
				this._disposed = true;

				//write out the hidden surface form routes
				_textWriter.Write("<input name='uformpostroutevals' type='hidden' value='" + _base64String + "' />");

				base.Dispose(disposing);
			}
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
		/// <returns></returns>
		public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName,
											   object additionalRouteVals,
											   object htmlAttributes)
		{
			return html.BeginUmbracoForm(action, controllerName, additionalRouteVals, htmlAttributes.ToDictionary<object>());
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

			var area = Umbraco.Core.Configuration.GlobalSettings.UmbracoMvcArea;
			return html.BeginUmbracoForm(action, controllerName, area, additionalRouteVals, htmlAttributes);
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
		/// <returns></returns>
		public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Type surfaceType,
											   object additionalRouteVals,
											   object htmlAttributes)
		{
			return html.BeginUmbracoForm(action, surfaceType, additionalRouteVals, htmlAttributes.ToDictionary<object>());
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
		/// <returns></returns>
		public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Type surfaceType,
											   object additionalRouteVals,
											   IDictionary<string, object> htmlAttributes)
		{
			Mandate.ParameterNotNullOrEmpty(action, "action");
			Mandate.ParameterNotNull(surfaceType, "surfaceType");

			var area = Umbraco.Core.Configuration.GlobalSettings.UmbracoMvcArea;
			var surfaceController = SurfaceControllerResolver.Current.SurfaceControllers
				.SingleOrDefault(x => x.Metadata.ControllerType == surfaceType);
			if (surfaceController == null)
				throw new InvalidOperationException("Could not find the surface controller of type " + surfaceType.FullName);
			if (!surfaceController.Metadata.AreaName.IsNullOrWhiteSpace())
			{
				//set the area to the plugin area
				area = surfaceController.Metadata.AreaName;
			}
			return html.BeginUmbracoForm(action, surfaceController.Metadata.ControllerName, area, additionalRouteVals, htmlAttributes);
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
		/// <returns></returns>
		public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName, string area,
											   object additionalRouteVals,
											   IDictionary<string, object> htmlAttributes)
		{
			Mandate.ParameterNotNullOrEmpty(area, "area");
			Mandate.ParameterNotNullOrEmpty(action, "action");
			Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");

			var formAction = UmbracoContext.Current.RequestUrl.ToString();
			return html.RenderForm(formAction, FormMethod.Post, htmlAttributes, controllerName, action, area, additionalRouteVals);
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

			var tagBuilder = new TagBuilder("form");
			tagBuilder.MergeAttributes(htmlAttributes);
			// action is implicitly generated, so htmlAttributes take precedence.
			tagBuilder.MergeAttribute("action", formAction);
			// method is an explicit parameter, so it takes precedence over the htmlAttributes. 
			tagBuilder.MergeAttribute("method", HtmlHelper.GetFormMethodString(method), true);
			var traditionalJavascriptEnabled = htmlHelper.ViewContext.ClientValidationEnabled && !htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled;
			if (traditionalJavascriptEnabled)
			{
				// forms must have an ID for client validation
				tagBuilder.GenerateId("form" + Guid.NewGuid().ToString("N"));
			}
			htmlHelper.ViewContext.Writer.Write(tagBuilder.ToString(TagRenderMode.StartTag));

			//new UmbracoForm:
			var theForm = new UmbracoForm(htmlHelper.ViewContext, surfaceController, surfaceAction, area, additionalRouteVals);

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