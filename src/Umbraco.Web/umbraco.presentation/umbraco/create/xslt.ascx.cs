using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Configuration;
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
            foreach (string fileName in Directory.GetFiles(IOHelper.MapPath(SystemDirectories.Umbraco + GetXsltTemplatePath()), "*.xslt"))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Name != "Clean.xslt")
                {
                    var liText = fi.Name.Replace(".xslt", "").SplitPascalCasing().ToFirstUpperInvariant();
                    xsltTemplate.Items.Add(new ListItem(liText, fi.Name));
                }
            }

        }

        private static string GetXsltTemplatePath()
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
            {
                return "/xslt/templates";
            }
            else
            {
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

                var xsltName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? xsltTemplate.SelectedValue :
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

        /// <summary>
        /// rename control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox rename;

        /// <summary>
        /// RequiredFieldValidator1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RequiredFieldValidator RequiredFieldValidator1;

        /// <summary>
        /// xsltTemplate control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.ListBox xsltTemplate;

        /// <summary>
        /// createMacro control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBox createMacro;

        /// <summary>
        /// Textbox1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox Textbox1;

        /// <summary>
        /// sbmt control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button sbmt;
    }
}
