using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;

namespace umbraco.presentation.masterpages
{
	public partial class umbracoDialog : System.Web.UI.MasterPage
	{

		public bool reportModalSize { get; set; }
		public static new event MasterPageLoadHandler Load;
		public new static event MasterPageLoadHandler Init;

		protected void Page_Load(object sender, EventArgs e)
		{
			ClientLoader.DataBind();
			ScriptManager.RegisterStartupScript(Page, Page.GetType(), "setRoot", "UmbClientMgr.setUmbracoPath(\"" + IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "\");", true);
			FireOnLoad(e);
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (Init != null)
			{
				Init(this, e);
			}
		}


		protected virtual void FireOnLoad(EventArgs e)
		{
			if (Load != null)
			{
				Load(this, e);
			}
		}

		/// <summary>
		/// Head1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.HtmlControls.HtmlHead Head1;

		/// <summary>
		/// ClientLoader control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.UmbracoClientDependencyLoader ClientLoader;

		/// <summary>
		/// CssInclude1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.CssInclude CssInclude1;

		/// <summary>
		/// JsInclude1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

		/// <summary>
		/// JsInclude3 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude3;

		/// <summary>
		/// JsInclude6 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude6;

		/// <summary>
		/// JsInclude4 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude4;

		/// <summary>
		/// JsInclude2 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude2;

		/// <summary>
		/// head control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.ContentPlaceHolder head;

		/// <summary>
		/// form1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.HtmlControls.HtmlForm form1;

		/// <summary>
		/// ScriptManager1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.ScriptManager ScriptManager1;

		/// <summary>
		/// body control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.ContentPlaceHolder body;

		/// <summary>
		/// footer control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.ContentPlaceHolder footer;
	}
}
