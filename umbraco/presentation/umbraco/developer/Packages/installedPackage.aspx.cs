using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.macro;
using runtimeMacro = umbraco.macro;
using System.Xml;

namespace umbraco.presentation.developer.packages
{
    public partial class installedPackage : BasePages.UmbracoEnsuredPage
    {
        private cms.businesslogic.packager.InstalledPackage pack;
        private cms.businesslogic.packager.repositories.Repository repo = new global::umbraco.cms.businesslogic.packager.repositories.Repository();

        protected void Page_Load(object sender, EventArgs e)
        {

            if (Request.QueryString["id"] != null)
            {
                pack = cms.businesslogic.packager.InstalledPackage.GetById(int.Parse(Request.QueryString["id"]));

                lt_packagename.Text = pack.Data.Name;
                lt_packageVersion.Text = pack.Data.Version;
                lt_packageAuthor.Text = pack.Data.Author;
                lt_readme.Text = library.ReplaceLineBreaks( pack.Data.Readme );

                bt_confirmUninstall.Attributes.Add("onClick", "jQuery('#buttons').hide(); jQuery('#loadingbar').show();; return true;");


                if (!Page.IsPostBack)
                {
                    //temp list to contain failing items... 
                    List<string> tempList = new List<string>();

                    foreach (string str in pack.Data.Documenttypes)
                    {
                        int tId = 0;
                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                DocumentType dc = new DocumentType(tId);
                                if (dc != null)
                                {
                                    ListItem li = new ListItem(dc.Text, dc.Id.ToString());
                                    li.Selected = true;
                                    documentTypes.Items.Add(li);
                                }
                                else
                                {
                                    tempList.Add(str);
                                }
                            }
                            catch
                            {
                                tempList.Add(str);
                            }
                        }
                    }
                    //removing failing documentTypes items from the uninstall manifest
                    syncLists(pack.Data.Documenttypes, tempList);


                    foreach (string str in pack.Data.Templates)
                    {
                        int tId = 0;
                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                Template t = new Template(tId);
                                if (t != null)
                                {
                                    ListItem li = new ListItem(t.Text, t.Id.ToString());
                                    li.Selected = true;
                                    templates.Items.Add(li);
                                }
                                else
                                {
                                    tempList.Add(str);
                                }
                            }
                            catch
                            {
                                tempList.Add(str);
                            }
                        }
                    }
                    //removing failing template items from the uninstall manifest
                    syncLists(pack.Data.Templates, tempList);

                    foreach (string str in pack.Data.Stylesheets)
                    {
                        int tId = 0;
                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                StyleSheet s = new StyleSheet(tId);
                                if (s != null)
                                {
                                    ListItem li = new ListItem(s.Text, s.Id.ToString());
                                    li.Selected = true;
                                    stylesheets.Items.Add(li);
                                }
                                else
                                {
                                    tempList.Add(str);
                                }
                            }
                            catch
                            {
                                tempList.Add(str);
                            }
                        }
                    }
                    //removing failing stylesheet items from the uninstall manifest
                    syncLists(pack.Data.Stylesheets, tempList);

                    foreach (string str in pack.Data.Macros)
                    {
                        int tId = 0;
                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                Macro m = new Macro(tId);
                                if (m != null && m.Properties != null)
                                { //Macros need an extra check to see if they actually exists. For some reason the macro does not return null, if the id is not found... 
                                    ListItem li = new ListItem(m.Name, m.Id.ToString());
                                    li.Selected = true;
                                    macros.Items.Add(li);
                                }
                                else
                                {
                                    tempList.Add(str);
                                }
                            }
                            catch
                            {
                                tempList.Add(str);
                            }
                        }
                    }
                    //removing failing macros items from the uninstall manifest
                    syncLists(pack.Data.Macros, tempList);

                    foreach (string str in pack.Data.Files)
                    {
                        try
                        {

                            if (!String.IsNullOrEmpty(str) && System.IO.File.Exists(Server.MapPath(str)))
                            {
                                ListItem li = new ListItem(str, str);
                                li.Selected = true;
                                files.Items.Add(li);
                            }
                            else
                            {
                                tempList.Add(str);
                            }

                        }
                        catch
                        {
                            tempList.Add(str);
                        }
                    }

                    //removing failing files from the uninstall manifest
                    syncLists(pack.Data.Files, tempList);

                    foreach (string str in pack.Data.DictionaryItems)
                    {
                        int tId = 0;

                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                cms.businesslogic.Dictionary.DictionaryItem di = new global::umbraco.cms.businesslogic.Dictionary.DictionaryItem(tId);

                                if (di != null)
                                {
                                    ListItem li = new ListItem(di.key, di.id.ToString());
                                    li.Selected = true;

                                    dictionaryItems.Items.Add(li);
                                }
                                else
                                {
                                    tempList.Add(str);
                                }
                            }
                            catch
                            {
                                tempList.Add(str);
                            }
                        }
                    }

                    //removing failing files from the uninstall manifest
                    syncLists(pack.Data.DictionaryItems, tempList);


                    foreach (string str in pack.Data.DataTypes)
                    {
                        int tId = 0;

                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                cms.businesslogic.datatype.DataTypeDefinition dtd = new global::umbraco.cms.businesslogic.datatype.DataTypeDefinition(tId);

                                if (dtd != null)
                                {
                                    ListItem li = new ListItem(dtd.Text, dtd.Id.ToString());
                                    li.Selected = true;

                                    dataTypes.Items.Add(li);
                                }
                                else
                                {
                                    tempList.Add(str);
                                }
                            }
                            catch
                            {
                                tempList.Add(str);
                            }
                        }
                    }

                    //removing failing files from the uninstall manifest
                    syncLists(pack.Data.DataTypes, tempList);

                    //save the install manifest, so even tho the user doesn't uninstall, it stays uptodate.
                    pack.Save();


                    //Look for updates on packages.
                    if (!String.IsNullOrEmpty(pack.Data.RepositoryGuid) && !String.IsNullOrEmpty(pack.Data.PackageGuid))
                    {
                        try
                        {
                            
                            repo = cms.businesslogic.packager.repositories.Repository.getByGuid(pack.Data.RepositoryGuid);

                            if (repo != null)
                            {
                                hl_packageRepo.Text = repo.Name;
                                hl_packageRepo.NavigateUrl = "BrowseRepository.aspx?repoGuid=" + repo.Guid;
                                pp_repository.Visible = true;
                            }

                            cms.businesslogic.packager.repositories.Package repoPackage = repo.Webservice.PackageByGuid(pack.Data.PackageGuid);

                            if (repoPackage != null) {
                                if (repoPackage.HasUpgrade && repoPackage.UpgradeVersion != pack.Data.Version) {
                                    bt_update.Visible = true;
                                    bt_update.Text = "Update available: version: " + repoPackage.UpgradeVersion;
                                    lt_upgradeReadme.Text = repoPackage.UpgradeReadMe;
                                    bt_gotoUpgrade.OnClientClick = "window.location.href = 'browseRepository.aspx?url=" + repoPackage.Url + "'; return true;";
                                    lt_noUpdate.Visible = false;
                                } else {
                                    bt_update.Visible = false;
                                    lt_noUpdate.Visible = true;
                                }

                                if (!string.IsNullOrEmpty(repoPackage.Demo)) {
                                    lb_demoLink.OnClientClick = "openDemo(this, '" + pack.Data.PackageGuid + "'); return false;";
                                    pp_documentation.Visible = true;
                                }
                                    
                                if(!string.IsNullOrEmpty(repoPackage.Documentation)) {
                                    hl_docLink.NavigateUrl = repoPackage.Documentation;
                                    hl_docLink.Target = "_blank";
                                    pp_documentation.Visible = true;
                                }
                            }
                        }
                        catch
                        {
                            bt_update.Visible = false;
                            lt_noUpdate.Visible = true;
                        }
                    }


                    bool deletePackage = true;
                    //sync the UI to match what is in the package
                    if (macros.Items.Count == 0)
                        pp_macros.Visible = false;
                    else
                        deletePackage = false;

                    if (documentTypes.Items.Count == 0)
                        pp_docTypes.Visible = false;
                    else
                        deletePackage = false;

                    if (files.Items.Count == 0)
                        pp_files.Visible = false;
                    else
                        deletePackage = false;

                    if (templates.Items.Count == 0)
                        pp_templates.Visible = false;
                    else
                        deletePackage = false;

                    if (stylesheets.Items.Count == 0)
                        pp_css.Visible = false;
                    else
                        deletePackage = false;

                    if (dictionaryItems.Items.Count == 0)
                        pp_di.Visible = false;
                    else
                        deletePackage = false;

                    if (dataTypes.Items.Count == 0)
                        pp_dt.Visible = false;
                    else
                        deletePackage = false;


                    if (deletePackage)
                    {
                        pane_noItems.Visible = true;
                        bt_uninstall.Visible = false;
                        pane_uninstall.Visible = false;
                    }
                }
            }
        }

        private void syncLists(List<string> list, List<string> removed)
        {
            foreach (string str in removed)
            {
                list.Remove(str);
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (String.IsNullOrEmpty(list[i].Trim()))
                    list.RemoveAt(i);
            }

            removed.Clear();
        }

        protected void delPack(object sender, EventArgs e)
        {
            pack.Delete();
            packageUninstalled.Visible = true;
            installedPackagePanel.Visible = false;
        }


        protected void confirmUnInstall(object sender, EventArgs e)
        {

            bool refreshCache = false;

            //Uninstall Stylesheets
            foreach (ListItem li in stylesheets.Items)
            {
                if (li.Selected)
                {
                    int nId;

                    if (int.TryParse(li.Value, out nId))
                    {
                        StyleSheet s = new StyleSheet(nId);
                        if (s != null)
                        {
                            s.delete();
                            pack.Data.Stylesheets.Remove(nId.ToString());
                        }
                    }
                }
            }

            //Uninstall templates
            foreach (ListItem li in templates.Items)
            {
                if (li.Selected)
                {
                    int nId;

                    if (int.TryParse(li.Value, out nId))
                    {
                        Template s = new Template(nId);
                        if (s != null)
                        {
                            s.RemoveAllReferences();
                            s.delete();
                            pack.Data.Templates.Remove(nId.ToString());
                        }
                    }
                }
            }

            //Uninstall macros
            foreach (ListItem li in macros.Items)
            {
                if (li.Selected)
                {
                    int nId;

                    if (int.TryParse(li.Value, out nId))
                    {
                        Macro s = new Macro(nId);
                        if (s != null && !String.IsNullOrEmpty(s.Name))
                        {
                            // remove from cache
                            new runtimeMacro(s.Id).removeFromCache();
                            s.Delete();
                            pack.Data.Macros.Remove(nId.ToString());

                        }
                    }
                }
            }

            //Remove files

            foreach (ListItem li in files.Items)
            {
                if (li.Selected)
                {
                    string filePath = HttpContext.Current.Server.MapPath("/" + li.Value.Trim('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        pack.Data.Files.Remove(li.Value);
                    }
                }
            }

            //Remove Document types
            foreach (ListItem li in documentTypes.Items)
            {
                if (li.Selected)
                {
                    int nId;

                    if (int.TryParse(li.Value, out nId))
                    {
                        DocumentType s = new DocumentType(nId);
                        if (s != null)
                        {
                            s.delete();
                            pack.Data.Documenttypes.Remove(nId.ToString());

                            // refresh content cache when document types are removed
                            refreshCache = true;

                        }
                    }
                }
            }

            //Remove Dictionary items
            foreach (ListItem li in dictionaryItems.Items)
            {
                if (li.Selected)
                {
                    int nId;

                    if (int.TryParse(li.Value, out nId))
                    {
                        cms.businesslogic.Dictionary.DictionaryItem di = new global::umbraco.cms.businesslogic.Dictionary.DictionaryItem(nId);
                        if (di != null)
                        {
                            di.delete();
                            pack.Data.DictionaryItems.Remove(nId.ToString());
                        }
                    }
                }
            }

            //Remove Data types
            foreach (ListItem li in dataTypes.Items)
            {
                if (li.Selected)
                {
                    int nId;

                    if (int.TryParse(li.Value, out nId))
                    {
                        cms.businesslogic.datatype.DataTypeDefinition dtd = new global::umbraco.cms.businesslogic.datatype.DataTypeDefinition(nId);
                        if (dtd != null)
                        {
                            dtd.delete();
                            pack.Data.DataTypes.Remove(nId.ToString());
                        }
                    }
                }
            }

            pack.Save();

            if (!isManifestEmpty())
            {
                Response.Redirect(Request.RawUrl);
            }
            else
            {
                BusinessLogic.Log.Add(global::umbraco.BusinessLogic.LogTypes.Debug, -1, "executing undo actions");

                // uninstall actions
                try {
                    System.Xml.XmlDocument actionsXml = new System.Xml.XmlDocument();
                    actionsXml.LoadXml("<Actions>" +  pack.Data.Actions + "</Actions>");

                    BusinessLogic.Log.Add(global::umbraco.BusinessLogic.LogTypes.Debug, -1, actionsXml.OuterXml);
                    foreach (XmlNode n in actionsXml.DocumentElement.SelectNodes("//Action")) {
                        try {
                            cms.businesslogic.packager.PackageAction.UndoPackageAction(pack.Data.Name, n.Attributes["alias"].Value, n);
                        } catch (Exception ex) {
                            BusinessLogic.Log.Add(global::umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
                        }
                    }
                } catch (Exception ex) {
                    BusinessLogic.Log.Add(global::umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
                }
                    
                pack.Delete();

                packageUninstalled.Visible = true;
                installedPackagePanel.Visible = false;
            }

            // refresh cache
            if (refreshCache) {
                library.RefreshContent();
            }

        }

        private bool isManifestEmpty()
        {

            pack.Data.Documenttypes.TrimExcess();
            pack.Data.Files.TrimExcess();
            pack.Data.Macros.TrimExcess();
            pack.Data.Stylesheets.TrimExcess();
            pack.Data.Templates.TrimExcess();
            pack.Data.DataTypes.TrimExcess();
            pack.Data.DictionaryItems.TrimExcess();

            List<List<string>> lists = new List<List<string>>();
            lists.Add(pack.Data.Documenttypes);
            lists.Add(pack.Data.Files);
            lists.Add(pack.Data.Macros);
            lists.Add(pack.Data.Stylesheets);
            lists.Add(pack.Data.Templates);
            lists.Add(pack.Data.DictionaryItems);
            lists.Add(pack.Data.DataTypes);



            foreach (List<string> list in lists)
            {

                foreach (string str in list)
                {
                    if (!String.IsNullOrEmpty(str.Trim()))
                        return false;
                }

            }

            return true;
        }

        protected override void OnInit(EventArgs e)
        {
                base.OnInit(e);
                Panel1.Text = ui.Text("treeHeaders", "installedPackages");
                pane_meta.Text = ui.Text("packager", "packageMetaData");
                pp_name.Text = ui.Text("packager", "packageName");
                pp_version.Text = ui.Text("packager", "packageVersion");
                pp_author.Text = ui.Text("packager", "packageAuthor");
                pp_repository.Text = ui.Text("packager", "packageRepository");
                pp_documentation.Text = ui.Text("packager", "packageDocumentation");
                pp_readme.Text = ui.Text("packager", "packageReadme");
                hl_docLink.Text = ui.Text("packager", "packageDocumentation");
                lb_demoLink.Text = ui.Text("packager", "packageDemonstration");

                pane_options.Text = ui.Text("packager", "packageOptions");
                lt_noUpdate.Text = ui.Text("packager", "packageNoUpgrades");
                bt_update.Text = ui.Text("packager", "packageUpgradeHeader");
                pp_upgradeInstruction.Text = ui.Text("packager", "packageUpgradeInstructions");
                bt_gotoUpgrade.Text = ui.Text("packager", "packageUpgradeDownload");

                pane_noItems.Text = ui.Text("packager", "packageNoItemsHeader");

                pane_uninstall.Text = ui.Text("packager", "packageUninstallHeader"); 
                bt_uninstall.Text = ui.Text("packager", "packageUninstallHeader");
                bt_deletePackage.Text = ui.Text("packager", "packageUninstallHeader");
                bt_confirmUninstall.Text = ui.Text("packager", "packageUninstallConfirm");

                pane_uninstalled.Text = ui.Text("packager", "packageUninstalledHeader");
        }
    }
}
