using System.Web;
using System.Web.UI;
using ClientDependency.Core;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.UI;
using Umbraco.Web;
using System;
using Umbraco.Web.UI.Controls;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using Umbraco.Web;
using System.Linq;
using UmbracoUserControl = Umbraco.Web.UI.Controls.UmbracoUserControl;

namespace umbraco.cms.presentation.create.controls
{
    /// <summary>
    ///		Summary description for simple.
    /// </summary>
    public partial class simple : UmbracoUserControl
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            sbmt.Text = ui.Text("create");
            rename.Attributes["placeholder"] = ui.Text("name");

            // Put user code to initialize the page here
        }

        protected void sbmt_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                int nodeId;
                if (int.TryParse(Request.QueryString["nodeId"], out nodeId) == false)
                    nodeId = -1;

                try
                {

                    var returnUrl = LegacyDialogHandler.Create(
                        new HttpContextWrapper(Context),
                        new User(Security.CurrentUser),
                        Request.GetItemAsString("nodeType"),
                            nodeId,
                            rename.Text.Trim(),
                            Request.QueryString.AsEnumerable().ToDictionary(x => x.Key, x => (object)x.Value));

                    BasePage.Current.ClientTools
                    .ChangeContentFrameUrl(returnUrl)
                    .ReloadActionNode(false, true)
                    .CloseModalWindow();
                }
                catch (Exception ex)
                {
                    CustomValidation.ErrorMessage = "* " + ex.Message;
                    CustomValidation.IsValid = false;
                }
            }

        }

        protected CustomValidator CustomValidation;

        /// <summary>
        /// RequiredFieldValidator1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RequiredFieldValidator RequiredFieldValidator1;

        /// <summary>
        /// rename control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox rename;

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
