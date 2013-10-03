using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.IO;

namespace umbraco.presentation.actions
{
    public partial class preview : BasePages.UmbracoEnsuredPage
    {

        public preview()
        {
            CurrentApp = BusinessLogic.DefaultApps.content.ToString();

        }
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            Document doc = new Document(int.Parse(helper.Request("id")));       
            Response.Redirect(IOHelper.ResolveUrl(string.Format("{0}/dialogs/preview.aspx?id= {1}", SystemDirectories.Umbraco, doc.Id)));
        }
    }
}
