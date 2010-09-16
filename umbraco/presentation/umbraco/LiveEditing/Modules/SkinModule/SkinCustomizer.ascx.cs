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
using umbraco.cms.businesslogic.template;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{

    public partial class SkinCustomizer : System.Web.UI.UserControl
    {
        private Skin ActiveSkin { get; set; }

        private List<Dependency> sDependencies = new List<Dependency>();

        private cms.businesslogic.packager.repositories.Repository repo;
        private string repoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public SkinCustomizer()
        {
            repo = cms.businesslogic.packager.repositories.Repository.getByGuid(repoGuid);
        }


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
                if (repo.HasConnection())
                {
                    try
                    {
                        rep_starterKitDesigns.DataSource = repo.Webservice.Skins(g.ToString());
                        rep_starterKitDesigns.DataBind();
                    }
                    catch (Exception ex)
                    {
                        BusinessLogic.Log.Add(BusinessLogic.LogTypes.Debug, -1, ex.ToString());

                        //ShowConnectionError();
                    }
                }
                else
                {
                    //ShowConnectionError();
                }
            }

            
        }

        protected void rep_starterKitDesigns_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem != null)
            {
                cms.businesslogic.packager.repositories.Skin s = (cms.businesslogic.packager.repositories.Skin)e.Item.DataItem;

                if (Skinning.IsSkinInstalled(s.RepoGuid))
                {
                    Button inst = (Button)e.Item.FindControl("Button1");
                    inst.Text = "Apply (already downloaded)";
                    inst.CommandName = "apply";
                    inst.CommandArgument = s.Text;

                }

                if (ActiveSkin.Name == s.Text)
                {
                    Button inst = (Button)e.Item.FindControl("Button1");
                    inst.Text = "Rollback (active skin)";
                    inst.CommandName = "remove";
                    inst.CommandArgument = s.Text;
                }
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

        protected void SelectStarterKitDesign(object sender, EventArgs e)
        {
            if (((Button)sender).CommandName == "apply")
            {
                Skin s = Skin.CreateFromName(((Button)sender).CommandArgument);
                Skinning.ActivateAsCurrentSkin(s);

                Page.Response.Redirect(library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())));
            }
            else if (((Button)sender).CommandName == "remove")
            {
                nodeFactory.Node n = nodeFactory.Node.GetCurrent();

                Template t = new Template(n.template);
                Skinning.RollbackSkin(t.Id);

                Page.Response.Redirect(library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())));
            }
            else
            {
                Guid kitGuid = new Guid(((Button)sender).CommandArgument);

                cms.businesslogic.packager.Installer installer = new cms.businesslogic.packager.Installer();

                if (repo.HasConnection())
                {
                    cms.businesslogic.packager.Installer p = new cms.businesslogic.packager.Installer();

                    string tempFile = p.Import(repo.fetch(kitGuid.ToString()));
                    p.LoadConfig(tempFile);
                    int pID = p.CreateManifest(tempFile, kitGuid.ToString(), repoGuid);

                    p.InstallFiles(pID, tempFile);
                    p.InstallBusinessLogic(pID, tempFile);
                    p.InstallCleanUp(pID, tempFile);

                    library.RefreshContent();

                    Skin s = Skin.CreateFromName(((Button)sender).CommandName);
                    Skinning.ActivateAsCurrentSkin(s);

                    Page.Response.Redirect(library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())));
                }
                else
                {
                    //ShowConnectionError();
                }
            }
        }

   
    }
}