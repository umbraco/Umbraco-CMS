using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.preview;
using umbraco.BusinessLogic;

namespace umbraco.presentation.dialogs
{
    public partial class Preview : BasePages.UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Document d = new Document(int.Parse(helper.Request("id")));
            bool includeChildren = !String.IsNullOrEmpty(UmbracoContext.Current.Request["children"]) ? true : false;
            PreviewContent pc = new PreviewContent(Guid.NewGuid());
            pc.PrepareDocument(base.getUser(), d, includeChildren);
            pc.SavePreviewSet();
            docLit.Text = d.Text;
            changeSetUrl.Text = pc.PreviewsetPath;
            StateHelper.SetCookieValue("PreviewSet", pc.PreviewSet.ToString());
        }
    }
}
