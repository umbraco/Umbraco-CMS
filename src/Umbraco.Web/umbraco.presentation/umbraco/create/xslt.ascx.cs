using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using Umbraco.Core;
using Umbraco.Web.UI;
using Umbraco.Core.IO;
using umbraco.cms.helpers;
using umbraco.BasePages;

namespace umbraco.presentation.create
{


    /// <summary>
    ///		Summary description for xslt.
    /// </summary>
    public partial class xslt : System.Web.UI.UserControl
    {
        protected System.Web.UI.WebControls.ListBox nodeType;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            sbmt.Text = ui.Text("create");
            foreach (string fileName in Directory.GetFiles(IOHelper.MapPath(SystemDirectories.Umbraco + getXsltTemplatePath()), "*.xslt"))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Name != "Clean.xslt")
                {
                    var liText = fi.Name.Replace(".xslt", "").SplitPascalCasing().ToFirstUpperInvariant();
                    xsltTemplate.Items.Add(new ListItem(liText, fi.Name));                    
                }
            }

        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion

        private static string getXsltTemplatePath()
        {
            if (UmbracoSettings.UseLegacyXmlSchema) {
                return "/xslt/templates";
            } else {
                return "/xslt/templates/schema2";
            }
        }

        protected void sbmt_Click(object sender, System.EventArgs e)
        {
            if (Page.IsValid)
            {
                var createMacroVal = 0;
                if (createMacro.Checked)
                    createMacroVal = 1;

                var xsltName = UmbracoSettings.UseLegacyXmlSchema ? xsltTemplate.SelectedValue :
                    Path.Combine("schema2", xsltTemplate.SelectedValue);

                var returnUrl = LegacyDialogHandler.Create(
                    new HttpContextWrapper(Context),
                    BasePage.Current.getUser(),
                    helper.Request("nodeType"),
                    createMacroVal,
                    xsltName + "|||" + rename.Text);

                BasePage.Current.ClientTools
                    .ChangeContentFrameUrl(returnUrl)
                    .ChildNodeCreated()
                    .CloseModalWindow();




            }

        }
    }
}
