using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using AjaxControlToolkit;
using umbraco.cms.businesslogic.web;

namespace umbraco.presentation.LiveEditing.Modules
{
    public class AddModule:Control
    {
        protected readonly ILiveEditingContext m_Context;

        protected ImageButton m_AddButton = new ImageButton();

        protected Panel m_AddModal = new Panel();
        protected ModalPopupExtender m_AddModalExtender = new ModalPopupExtender();

        public AddModule(ILiveEditingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (!context.Enabled)
                throw new ApplicationException("Live Editing is not enabled.");
            
            m_Context = context;
        }
       
         protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();

            


        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();


            Controls.Add(m_AddButton);
            m_AddButton.ID = "LeAddButton";
            m_AddButton.CssClass = "button";
            m_AddButton.ToolTip = "Add";
            m_AddButton.ImageUrl = String.Format("{0}/images/new.gif", GlobalSettings.Path);

            Controls.Add(m_AddModal);
            m_AddModal.ID = "LeAddModal";
            m_AddModal.CssClass = "modal";
            m_AddModal.Width = 300;
            m_AddModal.Attributes.Add("Style", "display: none");

            m_AddModal.Controls.Add(new LiteralControl("Hello world from add modal <br/>"));
           

            Controls.Add(m_AddModalExtender);
            m_AddModalExtender.ID = "ModalAdd";
            m_AddModalExtender.TargetControlID = m_AddButton.ID;
            m_AddModalExtender.PopupControlID = m_AddModal.ID;
            m_AddModalExtender.BackgroundCssClass = "modalBackground";
            //m_AddModalExtender.OkControlID = m_ConfirmAddButton.ID;
            //m_AddModalExtender.CancelControlID = m_CancelAddButton.ID;
            //m_AddModalExtender.OnOkScript = "okAddPage()";
        }
    }
}
