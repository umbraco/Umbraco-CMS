using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.LiveEditing.Controls;
using umbraco.BusinessLogic.Actions;
using ClientDependency.Core;
using umbraco.IO;
namespace umbraco.presentation.LiveEditing.Modules.DeleteModule
{
    [ClientDependency(200, ClientDependencyType.Javascript, "LiveEditing/Modules/DeleteModule/DeleteModule.js", "UmbracoRoot")]
    public class DeleteModule : BaseModule
    {
        protected ImageButton m_DeleteButton;

        protected Panel m_DeleteModal;

        protected Button m_ConfirmDeleteButton;
        protected Button m_CancelDeleteButton;

        public DeleteModule(LiveEditingManager manager)
            : base(manager)
        { }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            m_DeleteModal = new Panel();
            m_DeleteModal.ID = "LeDeleteModal";
            m_DeleteModal.Attributes.Add("Style", "display: none");

            m_ConfirmDeleteButton = new Button();
            m_ConfirmDeleteButton.Text = ui.GetText("ok");
            m_ConfirmDeleteButton.ID = "LeDeleteModalConfirm";
            m_ConfirmDeleteButton.CssClass = "modalbuton";
            m_ConfirmDeleteButton.OnClientClick = "DeleteModuleOk();";

            m_CancelDeleteButton = new Button();
            m_CancelDeleteButton.ID = "CancelDelete";
            m_CancelDeleteButton.Text = ui.GetText("cancel");
            m_CancelDeleteButton.CssClass = "modalbuton";

            m_DeleteModal.Controls.Add(new LiteralControl("<p>" + ui.GetText("confirmdelete") + "</p>"));
            m_DeleteModal.Controls.Add(m_ConfirmDeleteButton);
            m_DeleteModal.Controls.Add(new LiteralControl("&nbsp;"));
            m_DeleteModal.Controls.Add(m_CancelDeleteButton);

            Controls.Add(m_DeleteModal);

            m_DeleteButton = new ImageButton();
            m_DeleteButton.ID = "LeDeleteButton";
            m_DeleteButton.CssClass = "button";
            m_DeleteButton.ToolTip = ui.GetText("delete"); ;
            m_DeleteButton.ImageUrl = String.Format("{0}/LiveEditing/Modules/DeleteModule/delete.png", SystemDirectories.Umbraco);
            m_DeleteButton.Visible = UmbracoContext.Current.HasPermission(ActionDelete.Instance.Letter);
            m_DeleteButton.OnClientClick = "jQuery('#" + m_DeleteModal.ClientID + @"').ModalWindowShow('" + ui.GetText("delete") + "',true,300,200,50,0, ['.modalbuton'], null);return false;";

            Controls.Add(m_DeleteButton);
        }


        /// <summary>
        /// Handles the <c>MessageReceived</c> event of the manager.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected override void Manager_MessageReceived(object sender, MesssageReceivedArgs e)
        {
            switch (e.Type)
            {
                case "deletecontent":
                    Document currentPage = new Document(int.Parse(UmbracoContext.Current.PageId.ToString()));
                    string redirectUrl = "/";
                    try
                    {
                        redirectUrl = library.NiceUrl(currentPage.Parent.Id);
                    }
                    catch { }
                    library.UnPublishSingleNode(currentPage.Id);
                    currentPage.delete();
                    Page.Response.Redirect(redirectUrl);
                    break;
            }
        }
    }
}
