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

namespace umbraco.presentation.LiveEditing.Modules
{
    public class CssModule: Control
    {
        protected readonly ILiveEditingContext m_Context;

        protected ImageButton m_CssButton = new ImageButton();

        protected Panel m_CssModal = new Panel();
        protected ModalPopupExtender m_CssModalExtender = new ModalPopupExtender();

        public CssModule(ILiveEditingContext context)
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

            Controls.Add(m_CssButton);
            m_CssButton.ID = "LeCssButton";
            m_CssButton.CssClass = "button";
            m_CssButton.ToolTip = "Edit Css";
            m_CssButton.ImageUrl = String.Format("{0}/images/umbraco/settingCss.gif", GlobalSettings.Path);

            Controls.Add(m_CssModal);
            m_CssModal.ID = "LeCssModal";
            m_CssModal.CssClass = "modal";
            m_CssModal.Width = 300;
            m_CssModal.Attributes.Add("Style", "display: none");

            m_CssModal.Controls.Add(new LiteralControl("Hello world from css modal"));

            Controls.Add(m_CssModalExtender);
            m_CssModalExtender.TargetControlID = m_CssButton.ID;
            m_CssModalExtender.PopupControlID = m_CssModal.ID;
            m_CssModalExtender.BackgroundCssClass = "modalBackground";



        }
    }
}
