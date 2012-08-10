using System;
using System.Threading;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Umbraco.Web;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic;

namespace umbraco
{
	/// <summary>
	/// Summary description for WebForm1.
	/// </summary>
	/// 
	public partial class UmbracoDefault : Page
	{
		private page _upage = null;
		private DocumentRequest _docRequest = null;
		bool _validateRequest = true;

		const string TraceCategory = "UmbracoDefault";

		/// <summary>
		/// To turn off request validation set this to false before the PageLoad event. This equivalent to the validateRequest page directive
		/// and has nothing to do with "normal" validation controls. Default value is true.
		/// </summary>
		public bool ValidateRequest
		{
			get { return _validateRequest; }
			set { _validateRequest = value; }
		}

		// fixme - switch over to OnPreInit override
		void Page_PreInit(Object sender, EventArgs e)
		{
			Trace.Write(TraceCategory, "Begin PreInit");
			
			//TODO: This still a bunch of routing stuff being handled here, this all needs to be handled in the HttpModule instead

			// get the document request
			_docRequest = UmbracoContext.Current.DocumentRequest;

			// load request parameters
			// any reason other than liveEditing why we want to do this?!
			Guid requestVersion;
			if (!Guid.TryParse(Request["umbVersion"], out requestVersion))
				requestVersion = Guid.Empty;
			int requestId;
			if (!int.TryParse(Request["umbPageID"], out requestId))
				requestId = -1;

			if (requestId <= 0)
			{
				// asking for a different version of the default document
				if (requestVersion != Guid.Empty)
				{
					// security check
					var bp = new BasePages.UmbracoEnsuredPage();
					bp.ensureContext();
					requestId = _docRequest.NodeId;
				}
			}
			else
			{
				// asking for a specific document (and maybe a specific version)
				// security check
				var bp = new BasePages.UmbracoEnsuredPage();
				bp.ensureContext();
			}

			if (requestId <= 0)
			{
				// use the DocumentRequest if it has resolved
				// else it means that no lookup could find a document to render
				// or that the document to render has no template... 
				if (_docRequest.HasNode && _docRequest.HasTemplate)
				{
					_upage = new page(_docRequest);
					UmbracoContext.Current.HttpContext.Items["pageID"] = _docRequest.NodeId; // legacy - fixme
					var templatePath = umbraco.IO.SystemDirectories.Masterpages + "/" + _docRequest.Template.Alias.Replace(" ", "") + ".master"; // fixme - should be in .Template!
					this.MasterPageFile = templatePath; // set the template
				}
				else
				{
					//TODO: If there is no template but it has a node we shouldn't render a 404 I don't think

					// if we know we're 404, display the ugly message, else a blank page
					if (_docRequest.Is404)
						RenderNotFound();
					Response.End();
					return;
				}
			}
			else
			{
				// override the document = ignore the DocumentRequest
				// fixme - what if it fails?
				var document = requestVersion == Guid.Empty ? new Document(requestId) : new Document(requestId, requestVersion);
				_upage = new page(document);
				UmbracoContext.Current.HttpContext.Items["pageID"] = requestId; // legacy - fixme

				// must fall back to old code
				OnPreInitLegacy();
			}

			// reset the friendly path so it's used by forms, etc.
			Trace.Write(TraceCategory, string.Format("Reset url to \"{0}\"", UmbracoContext.Current.RequestUrl));
			Context.RewritePath(UmbracoContext.Current.RequestUrl.PathAndQuery);

			Context.Items.Add("pageElements", _upage.Elements); // legacy - fixme

			Trace.Write(TraceCategory, "End PreInit");
		}

		void OnPreInitLegacy()
		{
			if (_upage.Template == 0)
			{
				string custom404 = umbraco.library.GetCurrentNotFoundPageId();
				if (!String.IsNullOrEmpty(custom404))
				{
					XmlNode xmlNodeNotFound = content.Instance.XmlContent.GetElementById(custom404);
					if (xmlNodeNotFound != null)
					{
						_upage = new page(xmlNodeNotFound);
					}
				}
			}

			if (_upage.Template != 0)
			{
				this.MasterPageFile = template.GetMasterPageName(_upage.Template);

				//TODO: The culture stuff needs to all be set in the module

				string cultureAlias = null;
				for (int i = _upage.SplitPath.Length - 1; i > 0; i--)
				{
					var domains = Domain.GetDomainsById(int.Parse(_upage.SplitPath[i]));
					if (domains.Length > 0)
					{
						cultureAlias = domains[0].Language.CultureAlias;
						break;
					}
				}

				if (cultureAlias != null)
				{
					UmbracoContext.Current.HttpContext.Trace.Write("default.aspx", "Culture changed to " + cultureAlias);
					var culture = new System.Globalization.CultureInfo(cultureAlias);
					Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = culture;
				}
			}
			else
			{
				Response.StatusCode = 404;
				RenderNotFound();
				Response.End();
			}
		}

		//
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (ValidateRequest)
				Request.ValidateInput();

			// handle the infamous umbDebugShowTrace, etc
			Page.Trace.IsEnabled &= GlobalSettings.DebugMode && !String.IsNullOrWhiteSpace(Request["umbDebugShowTrace"]);
		}

		//
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
				Trace.Write("Runtime Engine", "Umbraco is running in preview mode.");

				if (Response.ContentType == "text/HTML") // ASP.NET default value
				{
					int pos = text.ToLower().IndexOf("</body>");
					if (pos > -1)
					{
						string htmlBadge =
							String.Format(UmbracoSettings.PreviewBadge,
								umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco),
								umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco_client),
								Server.UrlEncode(UmbracoContext.Current.HttpContext.Request.Path));

						text = text.Substring(0, pos) + htmlBadge + text.Substring(pos, text.Length - pos);
					}
				}
			}

			// render
			writer.Write(text);
		}

		//TODO: This should be removed, we should be handling all 404 stuff in the module and executing the 
		// DocumentNotFoundHttpHandler instead but we need to fix the above routing concerns so that this all
		// takes place in the Module.
		void RenderNotFound()
		{
			Context.Response.StatusCode = 404;

			Response.Write("<html><body><h1>Page not found</h1>");
			UmbracoContext.Current.HttpContext.Response.Write("<h3>No umbraco document matches the url '" + HttpUtility.HtmlEncode(UmbracoContext.Current.ClientUrl) + "'.</h3>");

			// fixme - should try to get infos from the DocumentRequest?

			Response.Write("<p>This page can be replaced with a custom 404. Check the documentation for \"custom 404\".</p>");
			Response.Write("<p style=\"border-top: 1px solid #ccc; padding-top: 10px\"><small>This page is intentionally left ugly ;-)</small></p>");
			Response.Write("</body></html>");
		}
	}
}
