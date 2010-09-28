using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.presentation.LiveEditing.Modules;
using ClientDependency.Core;
using System.Web.UI.WebControls;
using umbraco.presentation.LiveEditing.Controls;
using umbraco.IO;
using System.Web.UI;
using umbraco.cms.businesslogic.skinning;
using ClientDependency.Core.Controls;
using umbraco.presentation.umbraco.controls;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{
    [ClientDependency(200, ClientDependencyType.Javascript, "modal/modal.js", "UmbracoClient")]
    [ClientDependency(200, ClientDependencyType.Css, "modal/style.css", "UmbracoClient")]
    public class SkinModule : BaseModule
    {
        protected LabelButton m_SkinButton = new LabelButton();
        protected Panel m_SkinModal;

      
        public SkinModule(LiveEditingManager manager)
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

            m_SkinModal = new Panel();
            m_SkinModal.ID = "LeSkinModal";
            m_SkinModal.Attributes.Add("style", "display: none");

            m_SkinModal.Controls.Add(new UserControl().LoadControl(String.Format("{0}/LiveEditing/Modules/SKinModule/SkinCustomizer.ascx", SystemDirectories.Umbraco)));

            Controls.Add(m_SkinModal);

            m_SkinButton.ID = "LeSkinButton";
            m_SkinButton.CssClass = "button";
            m_SkinButton.ToolTip = "Customize skin";
            m_SkinButton.ImageUrl = String.Format("{0}/LiveEditing/Modules/SKinModule/skin.png", SystemDirectories.Umbraco);

            string s = (Skin.CreateFromAlias(Skinning.GetCurrentSkinAlias(nodeFactory.Node.GetCurrent().template)) != null ? "setTasksClientScripts();" : "") + "jQuery('#" + m_SkinModal.ClientID + @"').show();" + "jQuery('#" + m_SkinModal.ClientID + @"').ModalWindowShow('" + ui.GetText("skin") + "',true,500,400,50,0, ['.modalbuton'], null);";

            m_SkinButton.OnClientClick = s +"return false;";

            Controls.Add(m_SkinButton);

            if (!string.IsNullOrEmpty(UmbracoContext.Current.Request["skinning"]))
            {
                ScriptManager.RegisterClientScriptBlock(
                   this,
                   this.GetType(),
                   "ShowSkinModule",
                   "function ShowSkinModule(){" + s + "}",
                   true);


                ClientDependencyLoader.Instance.RegisterDependency(500, "LiveEditing/Modules/SkinModule/js/SkinModuleShowOnStartup.js", "UmbracoRoot", ClientDependencyType.Javascript);
            }
        }
    }
}