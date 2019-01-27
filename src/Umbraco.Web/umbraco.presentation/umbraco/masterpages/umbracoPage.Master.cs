using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

//This is only in case an upgrade goes wrong and the /masterpages/ files are not copied over
//which would result in an error. so we have kept the old namespaces intact with references to new ones
using StackExchange.Profiling;
using Umbraco.Core.Configuration;
using Umbraco.Core.Profiling;
using Umbraco.Web;
using Umbraco.Web.UI.JavaScript;
using mp = umbraco.presentation.masterpages;
namespace umbraco.presentation.umbraco.masterpages
{
    public class umbracoPage : mp.umbracoPage { }
    public class umbracoDialog : mp.umbracoDialog { }
}

namespace umbraco.presentation.masterpages
{
    public delegate void MasterPageLoadHandler(object sender, System.EventArgs e);

    public partial class umbracoPage : System.Web.UI.MasterPage
    {

        public new static event MasterPageLoadHandler Load;
        public new static event MasterPageLoadHandler Init;

        protected void Page_Load(object sender, EventArgs e)
        {
            ClientLoader.DataBind();
            FireOnLoad(e);
        }


        protected override void Render(HtmlTextWriter writer)
        {
            // get base output
            var baseWriter = new StringWriter();
            base.Render(new HtmlTextWriter(baseWriter));
            var baseOutput = baseWriter.ToString();

            // profiling
            if (string.IsNullOrEmpty(Request.QueryString["umbDebug"]) == false && GlobalSettings.DebugMode)
            {
                var htmlHelper = new HtmlHelper(new ViewContext(), new ViewPage());
                baseOutput = baseOutput.Replace("</body>", htmlHelper.RenderProfiler() + "</body>");
            }

            // write modified output
            writer.Write(baseOutput);
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
        /// ClientLoader control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected UmbracoClientDependencyLoader ClientLoader;

        /// <summary>
        /// CssInclude1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.CssInclude CssInclude1;

        /// <summary>
        /// CssInclude2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.CssInclude CssInclude2;

        /// <summary>
        /// JsInclude1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

        /// <summary>
        /// JsInclude2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude2;

        /// <summary>
        /// JsInclude8 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude8;

        /// <summary>
        /// JsInclude9 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude9;

        /// <summary>
        /// JsInclude4 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude4;

        /// <summary>
        /// JsInclude5 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude5;

        /// <summary>
        /// JsInclude6 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude6;

        /// <summary>
        /// JsInclude7 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude7;

        /// <summary>
        /// JsInclude3 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude3;

        /// <summary>
        /// JsIncludeHotkeys control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsIncludeHotkeys;

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
