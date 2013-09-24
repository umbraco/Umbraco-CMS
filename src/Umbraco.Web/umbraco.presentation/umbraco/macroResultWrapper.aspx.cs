using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

// using System.Collections;

namespace umbraco.presentation
{
    /// <summary>
    /// Summary description for macroResultWrapper.
    /// </summary>
    [Obsolete("This is no longer used and will be removed from the codebase")]
    public partial class macroResultWrapper : BasePages.UmbracoEnsuredPage
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {

            int macroID = cms.businesslogic.macro.Macro.GetByAlias(helper.Request("umb_macroAlias")).Id;
            int pageID = int.Parse(helper.Request("umbPageId"));
            Guid pageVersion = new Guid(helper.Request("umbVersionId"));

            System.Web.HttpContext.Current.Items["macrosAdded"] = 0;
            System.Web.HttpContext.Current.Items["pageID"] = pageID.ToString();

            // Collect attributes
            Hashtable attributes = new Hashtable();
            foreach (string key in Request.QueryString.AllKeys)
            {
                if (key.IndexOf("umb_") > -1)
                {
                    attributes.Add(key.Substring(4, key.Length - 4), Request.QueryString[key]);
                }
            }


            page p = new page(pageID, pageVersion);
            macro m = macro.GetMacro(macroID);

            Control c = m.renderMacro(attributes, p.Elements, p.PageID);
            PlaceHolder1.Controls.Add(c);
        }


        /// <summary>
        /// Form1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlForm Form1;

        /// <summary>
        /// PlaceHolder1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder PlaceHolder1;
    }
}
