using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Umbraco.Web;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.preview;
using umbraco.BusinessLogic;

namespace umbraco.presentation.dialogs
{
    public partial class Preview : BasePages.UmbracoEnsuredPage
    {
        public Preview()
        {
            CurrentApp = DefaultApps.content.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var d = new Document(Request.GetItemAs<int>("id"));
            var pc = new PreviewContent(UmbracoUser, Guid.NewGuid(), false);
            pc.PrepareDocument(UmbracoUser, d, true);
            pc.SavePreviewSet();
            docLit.Text = d.Text;
            changeSetUrl.Text = pc.PreviewsetPath;
            pc.ActivatePreviewCookie();
            Response.Redirect("../../" + d.Id.ToString(CultureInfo.InvariantCulture) + ".aspx", true);
        }
    }
}
