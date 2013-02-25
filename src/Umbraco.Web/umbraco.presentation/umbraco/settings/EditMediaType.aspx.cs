using System;
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadMediaTypes>().Tree.Alias)
                    .SyncTree("-1,init," + helper.Request("id"), false);
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            if (e is controls.SaveClickEventArgs)
            {
                var sce = (controls.SaveClickEventArgs)e;

                if (sce.Message == "Saved")
                {
                    ClientTools.ShowSpeechBubble(speechBubbleIcon.save, "Mediatype saved", "Mediatype was successfully saved");
                }
                else if (sce.Message.Contains("Tab"))
                {
                    ClientTools.ShowSpeechBubble(sce.IconType, sce.Message, "");
                }
                else
                {
                    ClientTools.ShowSpeechBubble(sce.IconType, sce.Message, "");
                }

                return true;
            }

            return false;
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
