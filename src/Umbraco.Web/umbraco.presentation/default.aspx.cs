using System;
using System.Threading;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic;

namespace umbraco
{
	/// <summary>
	/// The codebehind class for the main default.aspx page that does the webforms rendering in Umbraco
	/// </summary>
	/// <remarks>
	/// We would move this to the UI project but there is a public API property and some protected properties which people may be using so 
	/// we cannot move it.
	/// </remarks>
	public class UmbracoDefault : Page
	{
		private page _upage;
		private DocumentRequest _docRequest;
		bool _validateRequest = true;

		/// <summary>
		/// To turn off request validation set this to false before the PageLoad event. This equivalent to the validateRequest page directive
		/// and has nothing to do with "normal" validation controls. Default value is true.
		/// </summary>
		public bool ValidateRequest
		{
			get { return _validateRequest; }
			set { _validateRequest = value; }
		}

		protected override void OnPreInit(EventArgs e)
		{
			base.OnPreInit(e);

			// get the document request and the page
			_docRequest = UmbracoContext.Current.DocumentRequest;
			_upage = _docRequest.GetUmbracoPage();

			//we need to check this for backwards compatibility in case people still arent' using master pages
			if (UmbracoSettings.UseAspNetMasterPages)
			{
				var args = new RequestInitEventArgs()
				{
					Page = _upage,
					PageId = _upage.PageID,
					Context = Context
				};
				FireBeforeRequestInit(args);

				//if we are cancelling then return and don't proceed
				if (args.Cancel) return;

				var templatePath = SystemDirectories.Masterpages + "/" + _docRequest.Template.Alias.Replace(" ", "") + ".master"; // fixme - should be in .Template!
				this.MasterPageFile = templatePath; // set the template

				// reset the friendly path so it's used by forms, etc.			
				Context.RewritePath(UmbracoContext.Current.RequestUrl.PathAndQuery);

				//fire the init finished event
				FireAfterRequestInit(args);	
			}
			
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			//This is only here for legacy if people arent' using master pages... 
			//TODO: We need to test that this still works!! Or do we ??
			if (!UmbracoSettings.UseAspNetMasterPages)
			{
				var args = new RequestInitEventArgs()
				{
					Page = _upage,
					PageId = _upage.PageID,
					Context = Context
				};
				FireBeforeRequestInit(args);

				//if we are cancelling then return and don't proceed
				if (args.Cancel) return;

				var pageHolder = new umbraco.layoutControls.umbracoPageHolder
					{
						ID = "umbPageHolder"
					};
				Page.Controls.Add(pageHolder);
				_upage.RenderPage(_upage.Template);
				var umbPageHolder = (layoutControls.umbracoPageHolder)Page.FindControl("umbPageHolder");
				umbPageHolder.Populate(_upage);

				//fire the init finished event
				FireAfterRequestInit(args);	
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (ValidateRequest)
				Request.ValidateInput();
			// handle the infamous umbDebugShowTrace, etc
			Page.Trace.IsEnabled &= GlobalSettings.DebugMode && !String.IsNullOrWhiteSpace(Request["umbDebugShowTrace"]);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			// do the original rendering
			TextWriter sw = new StringWriter();
			base.Render(new HtmlTextWriter(sw));
			string text = sw.ToString();

			// filter / parse internal links - although this should be done elsewhere!
			text = template.ParseInternalLinks(text);

			// filter / add preview banner
			if (UmbracoContext.Current.InPreviewMode)
			{
				LogHelper.Debug<UmbracoDefault>("Umbraco is running in preview mode.", Context.Trace);

				if (Response.ContentType == "text/HTML") // ASP.NET default value
				{
					int pos = text.ToLower().IndexOf("</body>");
					if (pos > -1)
					{
						string htmlBadge =
							String.Format(UmbracoSettings.PreviewBadge,
								IOHelper.ResolveUrl(SystemDirectories.Umbraco),
								IOHelper.ResolveUrl(SystemDirectories.UmbracoClient),
								Server.UrlEncode(UmbracoContext.Current.HttpContext.Request.Path));

						text = text.Substring(0, pos) + htmlBadge + text.Substring(pos, text.Length - pos);
					}
				}
			}

			// render
			writer.Write(text);
		}

		/// <summary>
		/// The preinit event handler
		/// </summary>
		public delegate void RequestInitEventHandler(object sender, RequestInitEventArgs e);
		/// <summary>
		/// occurs before the umbraco page is initialized for rendering.
		/// </summary>
		public static event RequestInitEventHandler BeforeRequestInit;
		/// <summary>
		/// Raises the <see cref="BeforeRequestInit"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected internal virtual void FireBeforeRequestInit(RequestInitEventArgs e)
		{
			if (BeforeRequestInit != null)
				BeforeRequestInit(this, e);
		}

		/// <summary>
		/// Occurs when [after save].
		/// </summary>
		public static event RequestInitEventHandler AfterRequestInit;
		/// <summary>
		/// Raises the <see cref="AfterRequestInit"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void FireAfterRequestInit(RequestInitEventArgs e)
		{
			if (AfterRequestInit != null)
				AfterRequestInit(this, e);

		}
	}

	public class RequestInitEventArgs : System.ComponentModel.CancelEventArgs
	{
		public page Page { get; internal set; }
		public HttpContext Context { get; internal set; }
		public string Url { get; internal set; }
		public int PageId { get; internal set; }
	}
}
