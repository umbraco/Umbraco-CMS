using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.skinning;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic;

namespace umbraco.presentation.umbraco.dialogs
{
    public partial class TemplateSkinning : System.Web.UI.Page
    {
        private int templateID = 0;

        private cms.businesslogic.packager.repositories.Repository repo;
        private string repoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public TemplateSkinning()
        {
            repo = cms.businesslogic.packager.repositories.Repository.getByGuid(repoGuid);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            templateID = int.Parse(Request["id"]);
            Template t = new Template(templateID);

            if (Skinning.StarterKitGuid(templateID).HasValue)
            {
                p_apply.Visible = true;

                string currentSkin = Skinning.GetCurrentSkinAlias(templateID);
                int templateRoot = FindTemplateRoot((CMSNode)t);

                dd_skins.Items.Add("Choose...");
                foreach (KeyValuePair<string,string> kvp in Skinning.AllowedSkins(templateRoot))
                {
                    ListItem li = new ListItem(kvp.Value, kvp.Key);
                    if (kvp.Key == currentSkin)
                        li.Selected = true;

                    dd_skins.Items.Add(li);
                }

                if (!string.IsNullOrEmpty(Skinning.GetCurrentSkinAlias(templateID)))
                {
                    ph_rollback.Visible = true;
                }
            }
        }

        private int FindTemplateRoot(CMSNode t)
        {
            if (t.ParentId < 0)
                return t.Id;
            else
                return FindTemplateRoot(t.Parent);
        }

        protected void openRepo(object sender, EventArgs e) {

            Guid? g = Skinning.StarterKitGuid(templateID);


            if (g == null || !Skinning.HasAvailableSkins(templateID))
            {
                bt_repo.Visible = false;
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

            p_apply.Visible = false;
            p_download.Visible = true;
        
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

                    if (cms.businesslogic.skinning.Skinning.GetAllSkins().Count > 0)
                    {
                        cms.businesslogic.skinning.Skinning.ActivateAsCurrentSkin(cms.businesslogic.skinning.Skinning.GetAllSkins()[0]);
                    }


                    Page.Response.Redirect(library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())));
                }
                else
                {
                    //ShowConnectionError();
                }
            }
        }

        protected void apply(object sender, EventArgs e) {

            if (dd_skins.SelectedIndex > 0)
            {
                Skin s = Skin.CreateFromAlias(dd_skins.SelectedValue);
                Skinning.ActivateAsCurrentSkin(s);
            }

        }
        protected void rollback(object sender, EventArgs e) {

            Skinning.RollbackSkin(templateID);
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

                if (Skin.CreateFromAlias(Skinning.GetCurrentSkinAlias(templateID)).Name == s.Text)
                {
                    Button inst = (Button)e.Item.FindControl("Button1");
                    inst.Text = "Rollback (active skin)";
                    inst.CommandName = "remove";
                    inst.CommandArgument = s.Text;
                }
            }

        }
    }
}