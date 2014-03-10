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

    }
}
