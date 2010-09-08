using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.skinning;
using System.Xml;
using System.Text;
using umbraco.interfaces.skinning;
using umbraco.IO;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{

    public partial class SkinCustomizer : System.Web.UI.UserControl
    {
        private Skin ActiveSkin { get; set; }

        private List<Dependency> sDependencies = new List<Dependency>();

        protected void Page_Load(object sender, EventArgs e)
        {            
            


            nodeFactory.Node n = nodeFactory.Node.GetCurrent();

            ActiveSkin = Skin.CreateFromAlias(Skinning.GetCurrentSkinAlias(n.template));

            //load dependencies
            if (ActiveSkin != null)
                LoadDependencies();
            else
            {
                //show skin selection
                pCustomizeSkin.Visible = false;
                ltCustomizeSkinStyle.Text = ltChangeSkinStyle.Text;
                ltChangeSkinStyle.Text = string.Empty;
            }
       
           LoadSkins();
        }

        protected void LoadSkins()
        {
            Guid? g = Skinning.StarterKitGuid(nodeFactory.Node.GetCurrent().template);


            if (g == null || !Skinning.HasAvailableSkins(nodeFactory.Node.GetCurrent().template))
            {
                pChangeSkin.Visible = false;
            }
            else
            {
                install.steps.Skinning.loadStarterKitDesigns ctrl =
                    (install.steps.Skinning.loadStarterKitDesigns)new UserControl().LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKitDesigns.ascx");
                ctrl.StarterKitGuid = (Guid)g;

                ph_skins.Controls.Add(ctrl);
            }

            
        }

       
        protected void LoadDependencies()
        {
            ph_dependencies.Controls.Clear();

            StringBuilder s = new StringBuilder();

            s.Append(@"
                var hasSetTasksClientScriptsRun = false; 
                function setTasksClientScripts(){ 
                    if(hasSetTasksClientScriptsRun == false){");


            foreach (Dependency d in ActiveSkin.Dependencies)
            {
                if (d.DependencyType != null)
                {
                    sDependencies.Add(d);

                    ph_dependencies.Controls.Add(new LiteralControl("<p class=\"dependency\">"));

                    Label lbl = new Label();

                    lbl.Text = d.Label;

                    Control ctrl = d.DependencyType.Editor;

                    lbl.AssociatedControlID = ctrl.ID;

                    ph_dependencies.Controls.Add(lbl);

                    ph_dependencies.Controls.Add(new LiteralControl("<br/>"));

                    ph_dependencies.Controls.Add(ctrl);

                    ph_dependencies.Controls.Add(new LiteralControl("</p>"));

                   

                    foreach (Task t in d.Tasks)
                    {                     

                        s.Append(t.TaskType.PreviewClientScript(
                            ctrl.ClientID,
                            d.DependencyType.ClientSidePreviewEventType(),
                            d.DependencyType.ClientSideGetValueScript()));
                       
                        //ScriptManager.RegisterClientScriptBlock(
                        //    this,
                        //    this.GetType(),
                        //    d.Label + "_" + t.TaskType.Name,
                        //    t.TaskType.PreviewClientScript(ctrl.ClientID,d.Properties),
                        //    true);

                    }

                   
                }

               
            }

            s.Append("hasSetTasksClientScriptsRun = true; }}");

            ScriptManager.RegisterClientScriptBlock(
                this,
                this.GetType(),
                "TasksClientScripts",
                s.ToString(),
                true);

        }

        protected void btnOk_Click(object sender, EventArgs e)
        {         
            ActiveSkin.SaveOutput();

            foreach (Dependency d in sDependencies)
            {
                if (d.DependencyType.Values.Count > 0)
                {
                    string output = d.DependencyType.Values[0].ToString();

                    foreach (Task t in d.Tasks)
                    {
                        TaskExecutionDetails details = t.TaskType.Execute(ParsePlaceHolders(t.Value, output));

                        if (details.TaskExecutionStatus == TaskExecutionStatus.Completed)
                        {
                            ActiveSkin.AddTaskHistoryNode(
                                t.TaskType.ToXml(details.OriginalValue,details.NewValue));
                        }
                    }
                }
            }

            
        }

        private string ParsePlaceHolders(string value,string output)
        {
            //parse ${Output}
            value = value.Replace("${Output}", output);

            return value;
        }

        protected void bt_ChangeSkin_Click(object sender, EventArgs e)
        {
           
        }
    }
}