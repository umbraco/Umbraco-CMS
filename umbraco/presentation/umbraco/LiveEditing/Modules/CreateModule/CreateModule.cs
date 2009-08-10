using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.LiveEditing.Controls;
using Content = umbraco.cms.businesslogic.Content;
using umbraco.BusinessLogic.Actions;
using ClientDependency.Core;
namespace umbraco.presentation.LiveEditing.Modules.CreateModule
{
	[ClientDependency(200, ClientDependencyType.Javascript, "LiveEditing/Modules/CreateModule/CreateModule.js", "UmbracoRoot")]
    public class CreateModule : BaseModule
    {
        //protected const string CreateModuleScriptFile = "{0}/LiveEditing/Modules/CreateModule/CreateModule.js";

        protected ImageButton m_CreateButton = new ImageButton();

        protected Panel m_CreateModal = new Panel();
        protected ModalPopupExtender m_CreateModalExtender = new ModalPopupExtender();

        protected TextBox m_NameTextBox = new TextBox();
        protected DropDownList m_AllowedDocTypesDropdown = new DropDownList();

        protected Button m_ConfirmCreateButton = new Button();
        protected Button m_CancelCreateButton = new Button();

        public CreateModule(LiveEditingManager manager)
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

            Controls.Add(m_CreateButton);
            m_CreateButton.ID = "LeCreateButton";
            m_CreateButton.CssClass = "button";
            m_CreateButton.ToolTip = "Create";
            m_CreateButton.ImageUrl = String.Format("{0}/LiveEditing/Modules/CreateModule/create.png", GlobalSettings.Path);
            m_CreateButton.Visible = UmbracoContext.Current.HasPermission(ActionNew.Instance.Letter);

            Controls.Add(m_CreateModal);
            m_CreateModal.ID = "LeCreateModal";
            m_CreateModal.CssClass = "modal";
            m_CreateModal.Width = 300;
            m_CreateModal.Attributes.Add("Style", "display: none");

            m_CreateModal.Controls.Add(new LiteralControl("<div class='modaltitle'>Create Page </div>"));
            m_CreateModal.Controls.Add(new LiteralControl("<div class='modalcontent'>"));

            m_CreateModal.Controls.Add(new LiteralControl("<p><label>Name:</label>"));
            m_CreateModal.Controls.Add(m_NameTextBox);
            m_CreateModal.Controls.Add(new LiteralControl("</p><p><label>Choose Document Type:</label>"));
            m_CreateModal.Controls.Add(m_AllowedDocTypesDropdown);
            FillAllowedDoctypes();

            m_CreateModal.Controls.Add(new LiteralControl("</p>"));

            m_CreateModal.Controls.Add(m_ConfirmCreateButton);
            m_CreateModal.Controls.Add(new LiteralControl("&nbsp;"));
            m_CreateModal.Controls.Add(m_CancelCreateButton);

            m_ConfirmCreateButton.Text = "Ok";
            m_ConfirmCreateButton.ID = "LeCreateModalConfirm";
            m_ConfirmCreateButton.CssClass = "modalbuton";

            m_CancelCreateButton.Text = "Cancel";
            m_CancelCreateButton.CssClass = "modalbuton";

            Controls.Add(m_CreateModalExtender);
            m_CreateModalExtender.ID = "ModalCreate";
            m_CreateModalExtender.TargetControlID = m_CreateButton.ID;
            m_CreateModalExtender.PopupControlID = m_CreateModal.ID;
            m_CreateModalExtender.BackgroundCssClass = "modalBackground";
            m_CreateModalExtender.OkControlID = m_ConfirmCreateButton.ID;
            m_CreateModalExtender.CancelControlID = m_CancelCreateButton.ID;
            m_CreateModalExtender.OnOkScript = string.Format("CreateModuleOk()");

            m_CreateModal.Controls.Add(new LiteralControl("</div>"));

            //ScriptManager.RegisterClientScriptInclude(this, GetType(), CreateModuleScriptFile, String.Format(CreateModuleScriptFile, GlobalSettings.Path));
        }

        private void FillAllowedDoctypes()
        {
            m_AllowedDocTypesDropdown.Items.Clear();
            int NodeId = (int)UmbracoContext.Current.PageId;

            int[] allowedIds = new int[0];
            if (NodeId > 0)
            {
                Content c = new Document(NodeId);
                allowedIds = c.ContentType.AllowedChildContentTypeIDs;
            }

            foreach (DocumentType dt in DocumentType.GetAllAsList())
            {
                ListItem li = new ListItem();
                li.Text = dt.Text;
                li.Value = dt.Id.ToString();

                if (NodeId > 0)
                {
                    foreach (int i in allowedIds) if (i == dt.Id)
                        {
                            m_AllowedDocTypesDropdown.Items.Add(li);
                         
                        }
                }
                
            }
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
                case "createcontent":
                    int userid = BasePages.UmbracoEnsuredPage.GetUserId(BasePages.UmbracoEnsuredPage.umbracoUserContextID);
                    DocumentType typeToCreate = new DocumentType(Convert.ToInt32(m_AllowedDocTypesDropdown.SelectedValue));
                    Document newDoc = Document.MakeNew(m_NameTextBox.Text, typeToCreate, new global::umbraco.BusinessLogic.User(userid), (int)UmbracoContext.Current.PageId);
                    newDoc.Publish(new global::umbraco.BusinessLogic.User(userid));
                    library.PublishSingleNode(newDoc.Id);
                    Page.Response.Redirect(library.NiceUrl(newDoc.Id));
                    break;
            }
        }
    }
}
