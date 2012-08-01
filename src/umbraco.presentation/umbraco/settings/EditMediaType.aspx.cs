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
using umbraco.cms.presentation.Trees;

namespace umbraco.cms.presentation.settings
{
    /// <summary>
    /// Summary description for EditMediaType.
    /// </summary>
    public partial class EditMediaType : BasePages.UmbracoEnsuredPage
    {

        public EditMediaType()
        {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();

        }
        protected void Page_Load(object sender, System.EventArgs e)
        {
            if (!IsPostBack)
            {
                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadMediaTypes>().Tree.Alias)
                    .SyncTree("-1,init," + helper.Request("id"), false);
            }
        }
        protected void tmp_OnSave(object sender, System.EventArgs e)
        {

        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {

            if (e is controls.SaveClickEventArgs)
            {
                controls.SaveClickEventArgs sce = (controls.SaveClickEventArgs)e;

                if (sce.Message == "Saved")
                {
                    int mtid = 0;

                    speechBubble(speechBubbleIcon.save, "Mediatype saved", "Mediatype was successfully saved");
                    if (int.TryParse(Request.QueryString["id"], out mtid))
                        new cms.businesslogic.media.MediaType(mtid).Save();

                }
                else if (sce.Message.Contains("Tab"))
                {
                    speechBubble(speechBubbleIcon.info, "Tab added", sce.Message);
                }
                else
                {
                    base.speechBubble(sce.IconType, sce.Message, "");
                }

                return true;
            }
            else
            {
                return false;
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion
    }
}
