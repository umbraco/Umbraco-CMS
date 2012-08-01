using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.LiveEditing.Controls;
using Content = umbraco.cms.businesslogic.Content;
using umbraco.BusinessLogic.Actions;
using ClientDependency.Core;
using umbraco.IO;
namespace umbraco.presentation.LiveEditing.Modules.CreateModule
{
	[ClientDependency(200, ClientDependencyType.Javascript, "LiveEditing/Modules/CreateModule/CreateModule.js", "UmbracoRoot")]
    [ClientDependency(200, ClientDependencyType.Javascript, "modal/modal.js", "UmbracoClient")]
    [ClientDependency(200, ClientDependencyType.Css, "modal/style.css", "UmbracoClient")]
    public class CreateModule : BaseModule
    {
        protected ImageButton m_CreateButton = new ImageButton();

        protected Panel m_CreateModal;

        protected TextBox m_NameTextBox;
        protected DropDownList m_AllowedDocTypesDropdown;

        protected Button m_ConfirmCreateButton;
        protected Button m_CancelCreateButton;

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

            m_NameTextBox = new TextBox();
            m_NameTextBox.ID = "NewContentName";

            m_AllowedDocTypesDropdown = new DropDownList();
            m_AllowedDocTypesDropdown.ID = "AllowedDocTypes";

            m_ConfirmCreateButton = new Button();
            m_ConfirmCreateButton.ID = "ConfirmCreateButton";

            m_CancelCreateButton = new Button();
            m_CancelCreateButton.ID = "CancelCreateButton";

            m_CreateModal = new Panel();
            m_CreateModal.ID = "LeCreateModal";
            m_CreateModal.Attributes.Add("style", "display: none");

            m_CreateModal.Controls.Add(new LiteralControl("<p><label>" + ui.GetText("name") + ":</label>"));
            m_CreateModal.Controls.Add(m_NameTextBox);
            m_CreateModal.Controls.Add(new LiteralControl("</p><p><label>" + ui.GetText("choose") + " " + ui.GetText("documentType") + ":</label>"));
            m_CreateModal.Controls.Add(m_AllowedDocTypesDropdown);
            FillAllowedDoctypes();

            m_CreateModal.Controls.Add(new LiteralControl("</p>"));

            m_CreateModal.Controls.Add(m_ConfirmCreateButton);
            m_CreateModal.Controls.Add(new LiteralControl("&nbsp;"));
            m_CreateModal.Controls.Add(m_CancelCreateButton);

            m_ConfirmCreateButton.Text = ui.GetText("ok");
            m_ConfirmCreateButton.ID = "LeCreateModalConfirm";
            m_ConfirmCreateButton.CssClass = "modalbuton";
            m_ConfirmCreateButton.OnClientClick = "CreateModuleOk();";

            m_CancelCreateButton.Text = ui.GetText("cancel");
            m_CancelCreateButton.CssClass = "modalbuton";

            m_CreateModal.Controls.Add(new LiteralControl("</div>"));

            Controls.Add(m_CreateModal);

            m_CreateButton.ID = "LeCreateButton";
            m_CreateButton.CssClass = "button";
            m_CreateButton.ToolTip = ui.GetText("create");
            m_CreateButton.ImageUrl = String.Format("{0}/LiveEditing/Modules/CreateModule/create.png", SystemDirectories.Umbraco);
            m_CreateButton.Visible = UmbracoContext.Current.HasPermission(ActionNew.Instance.Letter);
            m_CreateButton.OnClientClick = "jQuery('#" + m_CreateModal.ClientID + @"').ModalWindowShow('" + ui.GetText("create") + "',true,300,200,50,0, ['.modalbuton'], null);return false;";
            
            Controls.Add(m_CreateButton);
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
                    library.UpdateDocumentCache(newDoc.Id);
                    Page.Response.Redirect(library.NiceUrl(newDoc.Id), false);
                    break;
            }
        }
    }
}
