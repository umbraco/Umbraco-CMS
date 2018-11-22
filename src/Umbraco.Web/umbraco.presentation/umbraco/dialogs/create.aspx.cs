using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web;
using umbraco.cms.businesslogic;
using umbraco.cms.presentation;
using umbraco.presentation;
using umbraco.BusinessLogic.Actions;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;

namespace umbraco.dialogs
{
    /// <summary>
    /// Summary description for create.
    /// </summary>
    public partial class create : UmbracoEnsuredPage
    {
        protected Button ok;

        private string _app;
        protected string App
        {
            get
            {
                if (_app == null)
                {
                    _app = Request.CleanForXss("app");
                    //validate the app
                    if (BusinessLogic.Application.getAll().Any(x => x.alias.InvariantEquals(_app)) == false)
                    {
                        throw new InvalidOperationException("A requested app: " + Request.GetItemAsString("app") + " was not found");
                    }
                }
                return _app;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Put user code to initialize the page here
            if (Request.GetItemAsString("nodeId") == "")
            {
                var appType = ui.Text("sections", App).ToLower();
                pane_chooseNode.Text = ui.Text("create", "chooseNode", appType, UmbracoUser) + "?";

                DataBind();
            }
            else
            {
                int nodeId = Request.GetItemAs<int>("nodeId");
                //ensure they have access to create under this node!!
                if (App.InvariantEquals(Constants.Applications.Media) || CheckCreatePermissions(nodeId))
                {
                    var c = new CMSNode(nodeId);
                    path.Value = c.Path;
                    pane_chooseNode.Visible = false;
                    panel_buttons.Visible = false;
                    pane_chooseName.Visible = true;
                    var createDef = new XmlDocument();
                    var defReader = new XmlTextReader(Server.MapPath(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/config/create/UI.xml"));
                    createDef.Load(defReader);
                    defReader.Close();

                    // Find definition for current nodeType
                    XmlNode def = createDef.SelectSingleNode("//nodeType [@alias = '" + App + "']");
                    phCreate.Controls.Add(new UserControl().LoadControl(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + def.SelectSingleNode("./usercontrol").FirstChild.Value));
                }
                else
                {
                    PageNameHolder.type = uicontrols.Feedback.feedbacktype.error;
                    PageNameHolder.Text = ui.GetText("rights") + " " + ui.GetText("error");
                    JTree.DataBind();
                }
            }

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.WebServices) + "/cmsnode.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.WebServices) + "/legacyAjaxCalls.asmx"));
        }

        private bool CheckCreatePermissions(int nodeId)
        {
            return CurrentUser.GetPermissions(new CMSNode(nodeId).Path)
                .Contains(ActionNew.Instance.Letter.ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// path control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlInputHidden path;

        /// <summary>
        /// pane_chooseNode control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_chooseNode;

        /// <summary>
        /// JTree control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.controls.Tree.TreeControl JTree;

        /// <summary>
        /// panel_buttons control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel panel_buttons;

        /// <summary>
        /// PageNameHolder control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Feedback PageNameHolder;

        /// <summary>
        /// pane_chooseName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_chooseName;

        /// <summary>
        /// phCreate control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder phCreate;
    }
}
