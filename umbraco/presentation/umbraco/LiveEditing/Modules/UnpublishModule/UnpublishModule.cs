using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.LiveEditing.Controls;
using umbraco.BusinessLogic.Actions;
using ClientDependency.Core;
using umbraco.IO;
namespace umbraco.presentation.LiveEditing.Modules.UnpublishModule
{
    [ClientDependency(200, ClientDependencyType.Javascript, "LiveEditing/Modules/UnpublishModule/UnpublishModule.js", "UmbracoRoot")]
    public class UnpublishModule : BaseModule
    {
        protected ImageButton m_UnpublishButton;

        protected Panel m_UnpublishModal;

        protected Button m_ConfirmUnpublishButton;
        protected Button m_CancelDeleteButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnpublishModule"/> class.
        /// </summary>
        /// <param name="manager">The Live Editing manager.</param>
        public UnpublishModule(LiveEditingManager manager)
            : base(manager)
        { }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();

        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            m_UnpublishModal = new Panel();
            m_UnpublishModal.ID = "LeUnpublishModal";
            m_UnpublishModal.Attributes.Add("Style", "display: none");

            m_CancelDeleteButton = new Button();
            m_CancelDeleteButton.ID = "CancelUnpublish";
            m_CancelDeleteButton.Text = ui.GetText("cancel");
            m_CancelDeleteButton.CssClass = "modalbuton";

            m_ConfirmUnpublishButton = new Button();
            m_ConfirmUnpublishButton.Text = ui.GetText("ok");
            m_ConfirmUnpublishButton.ID = "LeUnpublishModalConfirm";
            m_ConfirmUnpublishButton.CssClass = "modalbuton";
            m_ConfirmUnpublishButton.OnClientClick = "UnpublishModuleOk();";

            m_UnpublishModal.Controls.Add(new LiteralControl("<p>" + ui.GetText("areyousure") + "</p>"));
            m_UnpublishModal.Controls.Add(m_ConfirmUnpublishButton);
            m_UnpublishModal.Controls.Add(new LiteralControl("&nbsp;"));
            m_UnpublishModal.Controls.Add(m_CancelDeleteButton);

            Controls.Add(m_UnpublishModal);

            m_UnpublishButton = new ImageButton();
            m_UnpublishButton.ID = "LeUnpublishButton";
            m_UnpublishButton.CssClass = "button";
            m_UnpublishButton.ToolTip = "Unpublish";
            m_UnpublishButton.ImageUrl = String.Format("{0}/LiveEditing/Modules/UnpublishModule/unpublish.png", SystemDirectories.Umbraco);
            m_UnpublishButton.Visible = UmbracoContext.Current.HasPermission(ActionUnPublish.Instance.Letter);
            m_UnpublishButton.OnClientClick = "jQuery('#" + m_UnpublishModal.ClientID + @"').ModalWindowShow('" + ui.GetText("unPublish") + "',true,300,200,50,0, ['.modalbuton'], null);return false;";

            Controls.Add(m_UnpublishButton);
        }


        /// <summary>
        /// Handles the <c>MessageReceived</c> event of the Live Editing manager.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected override void Manager_MessageReceived(object sender, MesssageReceivedArgs e)
        {
            switch (e.Type)
            {
                case "unpublishcontent":
                    Document currentPage = new Document(int.Parse(UmbracoContext.Current.PageId.ToString()));
                    library.UnPublishSingleNode(currentPage.Id);
                    currentPage.UnPublish();
                    string redirectUrl = "/";
                    try
                    {
                        redirectUrl = library.NiceUrl(currentPage.Parent.Id);
                    }
                    catch
                    {
                    }

                    Page.Response.Redirect(redirectUrl);
                    break;
            }
        }
    }
}
