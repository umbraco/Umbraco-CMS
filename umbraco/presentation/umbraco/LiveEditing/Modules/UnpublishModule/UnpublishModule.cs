using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.LiveEditing.Controls;
using umbraco.BusinessLogic.Actions;
using ClientDependency.Core;
namespace umbraco.presentation.LiveEditing.Modules.UnpublishModule
{
	[ClientDependency(200, ClientDependencyType.Javascript, "LiveEditing/Modules/UnpublishModule/UnpublishModule.js", "UmbracoRoot")]
    public class UnpublishModule : BaseModule
    {
         //protected const string UnpublishModuleScriptFile = "{0}/LiveEditing/Modules/UnpublishModule/UnpublishModule.js";

         protected ImageButton m_UnpublishButton = new ImageButton();

         protected Panel m_UnpublishModal = new Panel();
         protected ModalPopupExtender m_UnpublishModalExtender = new ModalPopupExtender();

        protected Button m_ConfirmDeleteButton = new Button();
        protected Button m_CancelDeleteButton = new Button();

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

            Controls.Add(m_UnpublishButton);
            m_UnpublishButton.ID = "LeUnpublishButton";
            m_UnpublishButton.CssClass = "button";
            m_UnpublishButton.ToolTip = "Unpublish";
            m_UnpublishButton.ImageUrl = String.Format("{0}/LiveEditing/Modules/UnpublishModule/unpublish.png", GlobalSettings.Path);
            m_UnpublishButton.Visible = UmbracoContext.Current.HasPermission(ActionUnPublish.Instance.Letter);

            Controls.Add(m_UnpublishModal);
            m_UnpublishModal.ID = "LeUnpublishModal";
            m_UnpublishModal.CssClass = "modal";
            m_UnpublishModal.Width = 300;
            m_UnpublishModal.Attributes.Add("Style", "display: none");

            m_UnpublishModal.Controls.Add(new LiteralControl("<div class='modaltitle'>Unpublish Page </div>"));
            m_UnpublishModal.Controls.Add(new LiteralControl("<div class='modalcontent'><p>Are you sure you want to unpublish this page? </p>"));

            m_UnpublishModal.Controls.Add(m_ConfirmDeleteButton);
            m_UnpublishModal.Controls.Add(new LiteralControl("&nbsp;"));
            m_UnpublishModal.Controls.Add(m_CancelDeleteButton);

            m_ConfirmDeleteButton.Text = "Ok";
            m_ConfirmDeleteButton.ID = "LeUnpublishModalConfirm";
            m_ConfirmDeleteButton.CssClass = "modalbuton";
      
            m_CancelDeleteButton.Text = "Cancel";
            m_CancelDeleteButton.CssClass = "modalbuton";

            Controls.Add(m_UnpublishModalExtender);
            m_UnpublishModalExtender.ID = "ModalUnpublish";
            m_UnpublishModalExtender.TargetControlID = m_UnpublishButton.ID;
            m_UnpublishModalExtender.PopupControlID = m_UnpublishModal.ID;
            m_UnpublishModalExtender.BackgroundCssClass = "modalBackground";
            m_UnpublishModalExtender.OkControlID = m_ConfirmDeleteButton.ID;
            m_UnpublishModalExtender.CancelControlID = m_CancelDeleteButton.ID;
            m_UnpublishModalExtender.OnOkScript = string.Format("UnpublishModuleOk()");

            m_UnpublishModal.Controls.Add(new LiteralControl("</div>"));

            //ScriptManager.RegisterClientScriptInclude(this, GetType(), UnpublishModuleScriptFile, String.Format(UnpublishModuleScriptFile, GlobalSettings.Path));
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
