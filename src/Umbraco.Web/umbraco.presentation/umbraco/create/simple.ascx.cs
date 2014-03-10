using System;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using Umbraco.Web;

namespace umbraco.cms.presentation.create.controls
{
    /// <summary>
    ///		Summary description for simple.
    /// </summary>
    public partial class simple : System.Web.UI.UserControl
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            sbmt.Text = ui.Text("create");
            // Put user code to initialize the page here
        }

        protected void sbmt_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                int nodeId;
                if (int.TryParse(Request.QueryString["nodeId"], out nodeId) == false)
                    nodeId = -1;

                if (Request.GetItemAsString("nodeId") != "init")
                    nodeId = int.Parse(Request.GetItemAsString("nodeId"));

                try
                {
                    var returnUrl = umbraco.presentation.create.dialogHandler_temp.Create(
                        Request.GetItemAsString("nodeType"),
                        nodeId,
                        rename.Text.Trim());

                    BasePage.Current.ClientTools
                    .ChangeContentFrameUrl(returnUrl)
                    .ChildNodeCreated()
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
