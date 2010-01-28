using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
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
        //protected const string DeleteModuleScriptFile = "{0}/LiveEditing/Modules/DeleteModule/DeleteModule.js";

        protected ImageButton m_DeleteButton = new ImageButton();

        protected Panel m_DeleteModal = new Panel();
        protected ModalPopupExtender m_DeleteModalExtender = new ModalPopupExtender();

        protected Button m_ConfirmDeleteButton = new Button();
        protected Button m_CancelDeleteButton = new Button();

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

            Controls.Add(m_DeleteButton);
            m_DeleteButton.ID = "LeDeleteButton";
            m_DeleteButton.CssClass = "button";
            m_DeleteButton.ToolTip = "Delete";
            m_DeleteButton.ImageUrl = String.Format("{0}/LiveEditing/Modules/DeleteModule/delete.png", SystemDirectories.Umbraco);
            m_DeleteButton.Visible = UmbracoContext.Current.HasPermission(ActionDelete.Instance.Letter);
     
            Controls.Add(m_DeleteModal);
            m_DeleteModal.ID = "LeDeleteModal";
            m_DeleteModal.CssClass = "modal";
            m_DeleteModal.Width = 300;
            m_DeleteModal.Attributes.Add("Style", "display: none");

            m_DeleteModal.Controls.Add(new LiteralControl("<div class='modaltitle'>Delete Page </div>"));
            m_DeleteModal.Controls.Add(new LiteralControl("<div class='modalcontent'><p>Are you sure you want to delete this page? </p>"));

            m_DeleteModal.Controls.Add(m_ConfirmDeleteButton);
            m_DeleteModal.Controls.Add(new LiteralControl("&nbsp;"));
            m_DeleteModal.Controls.Add(m_CancelDeleteButton);

            m_ConfirmDeleteButton.Text = "Ok";
            m_ConfirmDeleteButton.ID = "LeDeleteModalConfirm";
            m_ConfirmDeleteButton.CssClass = "modalbuton";
      
            m_CancelDeleteButton.Text = "Cancel";
            m_CancelDeleteButton.CssClass = "modalbuton";
          
            Controls.Add(m_DeleteModalExtender);
            m_DeleteModalExtender.ID = "ModalDelete";
            m_DeleteModalExtender.TargetControlID = m_DeleteButton.ID;
            m_DeleteModalExtender.PopupControlID = m_DeleteModal.ID;
            m_DeleteModalExtender.BackgroundCssClass = "modalBackground";
            m_DeleteModalExtender.OkControlID = m_ConfirmDeleteButton.ID;
            m_DeleteModalExtender.CancelControlID = m_CancelDeleteButton.ID;
            m_DeleteModalExtender.OnOkScript = string.Format("DeleteModuleOk()");

            m_DeleteModal.Controls.Add(new LiteralControl("</div>"));

            //ScriptManager.RegisterClientScriptInclude(this, GetType(), DeleteModuleScriptFile, String.Format(DeleteModuleScriptFile, umbraco.IO.SystemDirectories.Umbraco));
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
