using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using umbraco.controls;
using umbraco.presentation.controls.Extenders;
using umbraco.presentation.LiveEditing.Controls;
using umbraco.presentation.LiveEditing.Updates;
using umbraco.presentation.templateControls;
using UM = umbraco.cms.businesslogic.macro;
using umbraco.presentation.ClientDependency;
namespace umbraco.presentation.LiveEditing.Modules.MacroEditing
{
	[ClientDependency(200, ClientDependencyType.Javascript, "LiveEditing/Modules/MacroModule/MacroModule.js", "UmbracoRoot")]
    public class MacroModule : BaseModule
    {
        //protected const string MacroModuleScriptFile = "{0}/LiveEditing/Modules/MacroModule/MacroModule.js";

        private ImageButton m_MacroButton = new ImageButton();

        private DropDownList m_MacroDropDown = new DropDownList();
        private DropDownList m_ContainerDropDown = new DropDownList();
        private List<Panel> containers = new List<Panel>();
        private Button m_AddMacroButton = new Button();

        private Panel m_MacroModal = new Panel();

        private Panel m_AddMacroStep2 = new Panel();

        private macroParameterControl m_MacroParameter = new macroParameterControl();

        private ModalPopupExtender m_MacroModalExtender = new ModalPopupExtender();

        public MacroModule(LiveEditingManager manager)
            : base (manager)
        { }

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
            
            Controls.Add(m_MacroButton);
            m_MacroButton.ID = "LeMacroButton";
            m_MacroButton.CssClass = "button";
            m_MacroButton.ToolTip = "Insert macro";
            m_MacroButton.ImageUrl = String.Format("{0}/images/editor/insMacro.gif", GlobalSettings.Path);

            Controls.Add(m_MacroModal);
            m_MacroModal.ID = "LeMacroModal";
            m_MacroModal.CssClass = "modal";
            m_MacroModal.Width = 500;
            m_MacroModal.Attributes.Add("Style", "display: none");

            m_MacroModal.Controls.Add(new LiteralControl("<div class='modaltitle'>Insert macro</div>"));
            m_MacroModal.Controls.Add(new LiteralControl("<div class='modalcontent'>"));


            m_MacroModal.Controls.Add(m_MacroDropDown);
            m_MacroDropDown.CssClass = "dropdown";
            m_MacroDropDown.Items.Clear();
            m_MacroDropDown.Items.Add(new ListItem("Choose macro", "0"));
            foreach (UM.Macro macro in UM.Macro.GetAll())
            {
                m_MacroDropDown.Items.Add(new ListItem(macro.Name, macro.Alias));
            }
            m_MacroDropDown.AutoPostBack = true;
            m_MacroDropDown.SelectedIndexChanged += new EventHandler(m_MacroDropDown_SelectedIndexChanged);

            m_MacroModal.Controls.Add(new LiteralControl("<br/>"));

            m_MacroModal.Controls.Add(m_ContainerDropDown);
            m_ContainerDropDown.CssClass = "dropdown";
            m_ContainerDropDown.Items.Clear();
            m_ContainerDropDown.Items.Add(new ListItem("Choose container", "0"));
            foreach (Control ctrl in Page.Controls)
            {
                FindMacroHolderPanels(ctrl);

            }

            m_MacroModal.Controls.Add(new LiteralControl("<br/>"));
            m_MacroModal.Controls.Add(m_AddMacroButton);
            m_AddMacroButton.Text = "Add Macro";
            
            m_AddMacroButton.Click += new EventHandler(m_AddMacroButton_Click);



            Controls.Add(m_MacroModalExtender);
            m_MacroModalExtender.ID = "ModalMacro";
            m_MacroModalExtender.TargetControlID = m_MacroButton.ID;
            m_MacroModalExtender.PopupControlID = m_MacroModal.ID;
            m_MacroModalExtender.BackgroundCssClass = "modalBackground";
            m_MacroModalExtender.OkControlID = m_AddMacroButton.ID;
            m_MacroModalExtender.OnOkScript = "okAddMacro()";

            m_MacroModal.Controls.Add(new LiteralControl("</div>"));

            //ScriptManager.RegisterClientScriptInclude(this, GetType(), MacroModuleScriptFile, String.Format(MacroModuleScriptFile, GlobalSettings.Path));

            
        }

        void m_MacroDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_MacroModal.Controls.Add(m_AddMacroStep2);

            m_AddMacroStep2.Controls.Add(new LiteralControl("<h2>Macro Properties</h2>"));

            m_MacroParameter.MacroAlias = m_MacroDropDown.SelectedValue;

            m_AddMacroStep2.Controls.Add(m_MacroParameter);
           
            m_MacroModalExtender.Show();
        }

        void m_AddMacroButton_Click(object sender, EventArgs e)
        {
            containers.ForEach(delegate(Panel p)
            {
                if (p.ID == m_ContainerDropDown.SelectedItem.Value)
                {
                    Macro macrotoadd = new Macro();
                    macrotoadd.Alias = m_MacroDropDown.SelectedItem.Value;

                    p.Controls.Add(new LiteralControl(string.Format(
                                                      "<div id=\"DragDropContainer{0}\" class=\"widget\"><div id=\"DragDropHandle{0}\" class=\"widget_header\">handle</div>",
                                                      macrotoadd.Alias + macrotoadd.ID)));
                    p.Controls.Add(macrotoadd);
                    p.Controls.Add(new LiteralControl("</div>"));
                    ((UpdatePanel)p.Parent.Parent).Update();

                    
                    TemplateUpdate update = new TemplateUpdate();
                    LiveEditingContext.Updates.Add(update);
                }
            });
        }

        private void FindMacroHolderPanels(Control ctrl)
        {
            foreach (Control childControl in ctrl.Controls)
            {

                if (childControl is Panel)
                {
                    if (((Panel)childControl).CssClass == "macro_holder")
                    {
                        Panel holderPanel = (Panel)childControl;
                        containers.Add(holderPanel);
                        m_ContainerDropDown.Items.Add(new ListItem(childControl.ID, childControl.ID));

                        ((UpdatePanel)holderPanel.Parent.Parent).Triggers.Clear();
                        AsyncPostBackTrigger apt = new AsyncPostBackTrigger();
                        apt.ControlID = m_MacroButton.ID;
                        apt.EventName = "Click";
                        ((UpdatePanel)holderPanel.Parent.Parent).Triggers.Add(apt);

                        childControl.Controls.Add(new LiteralControl(string.Format(
                                                                    "<div id=\"DropCue{0}\" class=\"widget_dropcue\"></div>",
                                                                    childControl.ID)));

                        CustomDragDropExtender extender = new CustomDragDropExtender();
                        extender.TargetControlID = childControl.ID;
                        extender.DragItemClass = "widget";
                        extender.DragItemHandleClass = "widget_header";
                        extender.DropCueID = string.Format("DropCue{0}", childControl.ID);
                        extender.OnClientDrop = "MacroOnDrop";

                        childControl.Controls.Add(extender);
                    }
                }

                if (childControl.HasControls())
                {
                    FindMacroHolderPanels(childControl);
                }
            }

        }
    }
}
