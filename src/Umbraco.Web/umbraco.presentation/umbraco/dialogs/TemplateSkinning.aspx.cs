using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.skinning;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic;

namespace umbraco.presentation.umbraco.dialogs
{
    public partial class TemplateSkinning : BasePages.UmbracoEnsuredPage
    {
        private int _templateId = 0;

        private readonly cms.businesslogic.packager.repositories.Repository _repo;
        private const string RepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public TemplateSkinning()
        {
            CurrentApp = DefaultApps.settings.ToString();
            _repo = cms.businesslogic.packager.repositories.Repository.getByGuid(RepoGuid);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _templateId = int.Parse(Request["id"]);
            var t = new Template(_templateId);

            if (Skinning.StarterKitGuid(_templateId).HasValue)
            {
                p_apply.Visible = true;

                var currentSkin = Skinning.GetCurrentSkinAlias(_templateId);
                var templateRoot = FindTemplateRoot(t);

                dd_skins.Items.Add("Choose...");
                foreach (var kvp in Skinning.AllowedSkins(templateRoot))
                {
                    var li = new ListItem(kvp.Value, kvp.Key);
                    if (kvp.Key == currentSkin)
                        li.Selected = true;

                    dd_skins.Items.Add(li);
                }

                if (!string.IsNullOrEmpty(Skinning.GetCurrentSkinAlias(_templateId)))
                {
                    ph_rollback.Visible = true;
                }
            }
        }

        private int FindTemplateRoot(CMSNode t)
        {
            if (t.ParentId < 0)
                return t.Id;
            return FindTemplateRoot(t.Parent);
        }

        protected void openRepo(object sender, EventArgs e) {

            var g = Skinning.StarterKitGuid(_templateId);


            if (g == null || !Skinning.HasAvailableSkins(_templateId))
            {
                bt_repo.Visible = false;
            }
            else
            {
                if (_repo.HasConnection())
                {
                    try
                    {
                        rep_starterKitDesigns.DataSource = _repo.Webservice.Skins(g.ToString());
                        rep_starterKitDesigns.DataBind();
                    }
                    catch (Exception ex)
                    {
						LogHelper.Error<TemplateSkinning>("An error occurred", ex);

                        //ShowConnectionError();
                    }
                }
            }

            p_apply.Visible = false;
            p_download.Visible = true;
        
        }

        protected void SelectStarterKitDesign(object sender, EventArgs e)
        {
            if (((Button)sender).CommandName == "apply")
            {
                var s = Skin.CreateFromName(((Button)sender).CommandArgument);
                Skinning.ActivateAsCurrentSkin(s);

                Page.Response.Redirect(library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())));
            }
            else if (((Button)sender).CommandName == "remove")
            {
                var n = NodeFactory.Node.GetCurrent();

                var t = new Template(n.template);
                Skinning.RollbackSkin(t.Id);

                Page.Response.Redirect(library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())));
            }
            else
            {

                var kitGuid = new Guid(((Button)sender).CommandArgument);

                if (_repo.HasConnection())
                {
                    var p = new cms.businesslogic.packager.Installer();

                    var tempFile = p.Import(_repo.fetch(kitGuid.ToString()));
                    p.LoadConfig(tempFile);
                    var pId = p.CreateManifest(tempFile, kitGuid.ToString(), RepoGuid);

                    p.InstallFiles(pId, tempFile);
                    p.InstallBusinessLogic(pId, tempFile);
                    p.InstallCleanUp(pId, tempFile);

                    //NOTE: This seems excessive to have to re-load all content from the database here!?
                    library.RefreshContent();

                    if (Skinning.GetAllSkins().Count > 0)
                    {
                        Skinning.ActivateAsCurrentSkin(Skinning.GetAllSkins()[0]);
                    }


                    Page.Response.Redirect(library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())));
                }
            }
        }

        protected void apply(object sender, EventArgs e) {

            if (dd_skins.SelectedIndex > 0)
            {
                var s = Skin.CreateFromAlias(dd_skins.SelectedValue);
                Skinning.ActivateAsCurrentSkin(s);
            }

        }
        protected void rollback(object sender, EventArgs e) {

            Skinning.RollbackSkin(_templateId);
        }

        protected void rep_starterKitDesigns_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem != null)
            {
                var s = (cms.businesslogic.packager.repositories.Skin)e.Item.DataItem;

                if (Skinning.IsSkinInstalled(s.RepoGuid))
                {
                    var inst = (Button)e.Item.FindControl("Button1");
                    inst.Text = "Apply (already downloaded)";
                    inst.CommandName = "apply";
                    inst.CommandArgument = s.Text;

                }

                if (Skin.CreateFromAlias(Skinning.GetCurrentSkinAlias(_templateId)).Name == s.Text)
                {
                    var inst = (Button)e.Item.FindControl("Button1");
                    inst.Text = "Rollback (active skin)";
                    inst.CommandName = "remove";
                    inst.CommandArgument = s.Text;
                }
            }

        }
    }
}