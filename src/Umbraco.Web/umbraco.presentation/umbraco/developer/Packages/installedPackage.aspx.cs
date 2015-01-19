using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Umbraco.Core.IO;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using runtimeMacro = umbraco.macro;
using System.Xml;
using umbraco.cms.presentation.Trees;
using BizLogicAction = umbraco.BusinessLogic.Actions.Action;
using Macro = umbraco.cms.businesslogic.macro.Macro;
using Template = umbraco.cms.businesslogic.template.Template;

namespace umbraco.presentation.developer.packages
{
    public partial class installedPackage : BasePages.UmbracoEnsuredPage
    {
        public installedPackage()
        {
            CurrentApp = DefaultApps.developer.ToString();
        }

        private cms.businesslogic.packager.InstalledPackage _pack;
        private cms.businesslogic.packager.repositories.Repository _repo = new cms.businesslogic.packager.repositories.Repository();

        protected void Page_Load(object sender, EventArgs e)
        {

            if (Request.QueryString["id"] != null)
            {
                _pack = cms.businesslogic.packager.InstalledPackage.GetById(int.Parse(Request.QueryString["id"]));

                lt_packagename.Text = _pack.Data.Name;
                lt_packageVersion.Text = _pack.Data.Version;
                lt_packageAuthor.Text = _pack.Data.Author;
                lt_readme.Text = library.ReplaceLineBreaks( _pack.Data.Readme );

                bt_confirmUninstall.Attributes.Add("onClick", "jQuery('#buttons').hide(); jQuery('#loadingbar').show();; return true;");


                if (!Page.IsPostBack)
                {
                    //temp list to contain failing items... 
                    var tempList = new List<string>();

                    foreach (var str in _pack.Data.Documenttypes)
                    {
                        var tId = 0;
                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                var dc = new DocumentType(tId);
                                var li = new ListItem(dc.Text, dc.Id.ToString());
                                li.Selected = true;
                                documentTypes.Items.Add(li);
                            }
                            catch
                            {
                                tempList.Add(str);
                            }
                        }
                    }
                    //removing failing documentTypes items from the uninstall manifest
                    SyncLists(_pack.Data.Documenttypes, tempList);


                    foreach (var str in _pack.Data.Templates)
                    {
                        var tId = 0;
                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                var t = new Template(tId);
                                var li = new ListItem(t.Text, t.Id.ToString());
                                li.Selected = true;
                                templates.Items.Add(li);
                            }
                            catch
                            {
                                tempList.Add(str);
                            }
                        }
                    }
                    //removing failing template items from the uninstall manifest
                    SyncLists(_pack.Data.Templates, tempList);

                    foreach (string str in _pack.Data.Stylesheets)
                    {
                        int tId = 0;
                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                var s = new StyleSheet(tId);
                                ListItem li = new ListItem(s.Text, s.Id.ToString());
                                li.Selected = true;
                                stylesheets.Items.Add(li);
                            }
                            catch
                            {
                                tempList.Add(str);
                            }
                        }
                    }
                    //removing failing stylesheet items from the uninstall manifest
                    SyncLists(_pack.Data.Stylesheets, tempList);

                    foreach (var str in _pack.Data.Macros)
                    {
                        var tId = 0;
                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                var m = new Macro(tId);
                                if (!string.IsNullOrEmpty(m.Name))
                                { 
                                    //Macros need an extra check to see if they actually exists. For some reason the macro does not return null, if the id is not found... 
                                    var li = new ListItem(m.Name, m.Id.ToString());
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
                    SyncLists(_pack.Data.Macros, tempList);

                    foreach (var str in _pack.Data.Files)
                    {
                        try
                        {                            
                            if (!string.IsNullOrEmpty(str) && System.IO.File.Exists(IOHelper.MapPath(str) ))
                            {
                                var li = new ListItem(str, str);
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
                    SyncLists(_pack.Data.Files, tempList);

                    foreach (string str in _pack.Data.DictionaryItems)
                    {
                        var tId = 0;

                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                var di = new cms.businesslogic.Dictionary.DictionaryItem(tId);

                                var li = new ListItem(di.key, di.id.ToString());
                                li.Selected = true;

                                dictionaryItems.Items.Add(li);
                            }
                            catch
                            {
                                tempList.Add(str);
                            }
                        }
                    }

                    //removing failing files from the uninstall manifest
                    SyncLists(_pack.Data.DictionaryItems, tempList);


                    foreach (var str in _pack.Data.DataTypes)
                    {
                        var tId = 0;

                        if (int.TryParse(str, out tId))
                        {
                            try
                            {
                                var dtd = new cms.businesslogic.datatype.DataTypeDefinition(tId);

                                if (dtd != null)
                                {
                                    var li = new ListItem(dtd.Text, dtd.Id.ToString());
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
                    SyncLists(_pack.Data.DataTypes, tempList);

                    //save the install manifest, so even tho the user doesn't uninstall, it stays uptodate.
                    _pack.Save();


                    //Look for updates on packages.
                    if (!string.IsNullOrEmpty(_pack.Data.RepositoryGuid) && !string.IsNullOrEmpty(_pack.Data.PackageGuid))
                    {
                        try
                        {
                            
                            _repo = cms.businesslogic.packager.repositories.Repository.getByGuid(_pack.Data.RepositoryGuid);

                            if (_repo != null)
                            {
                                hl_packageRepo.Text = _repo.Name;
                                hl_packageRepo.NavigateUrl = "BrowseRepository.aspx?repoGuid=" + _repo.Guid;
                                pp_repository.Visible = true;
                            }

                            var repoPackage = _repo.Webservice.PackageByGuid(_pack.Data.PackageGuid);

                            if (repoPackage != null)
                            {
                                if (repoPackage.HasUpgrade && repoPackage.UpgradeVersion != _pack.Data.Version)
                                {
                                    pane_upgrade.Visible = true;
                                    lt_upgradeReadme.Text = repoPackage.UpgradeReadMe;
                                    bt_gotoUpgrade.OnClientClick = "window.location.href = 'browseRepository.aspx?url=" + repoPackage.Url + "'; return true;";
                                }
                                
                                if (!string.IsNullOrEmpty(repoPackage.Demo))
                                {
                                    lb_demoLink.OnClientClick = "openDemo(this, '" + _pack.Data.PackageGuid + "'); return false;";
                                    pp_documentation.Visible = true;
                                }

                                if (!string.IsNullOrEmpty(repoPackage.Documentation))
                                {
                                    hl_docLink.NavigateUrl = repoPackage.Documentation;
                                    hl_docLink.Target = "_blank";
                                    pp_documentation.Visible = true;
                                }
                            }
                        }
                        catch
                        {
                            pane_upgrade.Visible = false;
                        }
                    }


                    var deletePackage = true;
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
                        pane_uninstall.Visible = false;
                    }

                    // List the package version history [LK 2013-067-10]
                    Version v;
                    var packageVersionHistory = cms.businesslogic.packager.InstalledPackage.GetAllInstalledPackages()
                        .Where(x => x.Data.Id != _pack.Data.Id &&  string.Equals(x.Data.Name, _pack.Data.Name, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(x => Version.TryParse(x.Data.Version, out v) ? v : new Version());

                    if (packageVersionHistory != null && packageVersionHistory.Count() > 0)
                    {
                        rptr_versions.DataSource = packageVersionHistory;
                        rptr_versions.DataBind();

                        pane_versions.Visible = true;
                    }
                }
            }
        }

        private static void SyncLists(List<string> list, List<string> removed)
        {
            foreach (var str in removed)
            {
                list.Remove(str);
            }

            for (var i = 0; i < list.Count; i++)
            {
                if (String.IsNullOrEmpty(list[i].Trim()))
                    list.RemoveAt(i);
            }

            removed.Clear();
        }

        protected void delPack(object sender, EventArgs e)
        {
            _pack.Delete(UmbracoUser.Id);
            pane_uninstalled.Visible = true;
            pane_uninstall.Visible = false;
        }


        protected void confirmUnInstall(object sender, EventArgs e)
        {
            var refreshCache = false;

            //Uninstall Stylesheets
            foreach (ListItem li in stylesheets.Items)
            {
                if (li.Selected)
                {
                    int nId;

                    if (int.TryParse(li.Value, out nId))
                    {
                        var s = new StyleSheet(nId);
                        s.delete();
                        _pack.Data.Stylesheets.Remove(nId.ToString());
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
                        var s = new Template(nId);
                        s.RemoveAllReferences();
                        s.delete();
                        _pack.Data.Templates.Remove(nId.ToString());
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
                        var s = new Macro(nId);
                        if (!string.IsNullOrEmpty(s.Name))
                        {                            
                            s.Delete();
                        }

                        _pack.Data.Macros.Remove(nId.ToString());
                    }
                }
            }
            
            //Remove Document Types
            var contentTypes = new List<IContentType>();
            var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
            foreach (ListItem li in documentTypes.Items)
            {
                if (li.Selected)
                {
                    int nId;

                    if (int.TryParse(li.Value, out nId))
                    {
                        var contentType = contentTypeService.GetContentType(nId);
                        if (contentType != null)
                        {
                            contentTypes.Add(contentType);
                            _pack.Data.Documenttypes.Remove(nId.ToString(CultureInfo.InvariantCulture));
                            // refresh content cache when document types are removed
                            refreshCache = true;
                        }
                    }
                }
            }
            //Order the DocumentTypes before removing them
            if (contentTypes.Any())
            {
                var orderedTypes = (from contentType in contentTypes
                                    orderby contentType.ParentId descending, contentType.Id descending 
                                    select contentType);

                foreach (var contentType in orderedTypes)
                {
                    contentTypeService.Delete(contentType);
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
                        var di = new cms.businesslogic.Dictionary.DictionaryItem(nId);
                        di.delete();
                        _pack.Data.DictionaryItems.Remove(nId.ToString());
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
                        var dtd = new cms.businesslogic.datatype.DataTypeDefinition(nId);
                        dtd.delete();
                        _pack.Data.DataTypes.Remove(nId.ToString());
                    }
                }
            }

            _pack.Save();

            if (!IsManifestEmpty())
            {
                Response.Redirect(Request.RawUrl);
            }
            else
            {
                
                // uninstall actions
                try
                {
                    var actionsXml = new XmlDocument();
                    actionsXml.LoadXml("<Actions>" + _pack.Data.Actions + "</Actions>");

                    LogHelper.Debug<installedPackage>("executing undo actions: {0}", () => actionsXml.OuterXml);

                    foreach (XmlNode n in actionsXml.DocumentElement.SelectNodes("//Action"))
                    {
                        try
                        {
                            cms.businesslogic.packager.PackageAction.UndoPackageAction(_pack.Data.Name, n.Attributes["alias"].Value, n);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error<installedPackage>("An error occurred running undo actions", ex);
						}
					}
                }
                catch (Exception ex)
                {
                    LogHelper.Error<installedPackage>("An error occurred running undo actions", ex);
				}

	            //moved remove of files here so custom package actions can still undo
                //Remove files
                foreach (ListItem li in files.Items)
                {
                    if (li.Selected)
                    {
                        //here we need to try to find the file in question as most packages does not support the tilde char

                        var file = IOHelper.FindFile(li.Value);

                        var filePath = IOHelper.MapPath(file);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                            _pack.Data.Files.Remove(li.Value);
                        }
                    }
                }
                _pack.Save();
                _pack.Delete(UmbracoUser.Id);

                pane_uninstalled.Visible = true;
                pane_uninstall.Visible = false;

            }

            // refresh cache
            if (refreshCache)
            {
                library.RefreshContent();
            }

            //ensure that all tree's are refreshed after uninstall
            ClientTools.ClearClientTreeCache()
                .RefreshTree();

            TreeDefinitionCollection.Instance.ReRegisterTrees();

            BizLogicAction.ReRegisterActionsAndHandlers();
            
        }

        private bool IsManifestEmpty()
        {

            _pack.Data.Documenttypes.TrimExcess();
            _pack.Data.Files.TrimExcess();
            _pack.Data.Macros.TrimExcess();
            _pack.Data.Stylesheets.TrimExcess();
            _pack.Data.Templates.TrimExcess();
            _pack.Data.DataTypes.TrimExcess();
            _pack.Data.DictionaryItems.TrimExcess();

            var lists = new List<List<string>>
                {
                    _pack.Data.Documenttypes,
                    _pack.Data.Macros,
                    _pack.Data.Stylesheets,
                    _pack.Data.Templates,
                    _pack.Data.DictionaryItems, 
                    _pack.Data.DataTypes
                };

            //Not including files, since there might be assemblies that contain package actions
            //lists.Add(pack.Data.Files);

            return lists.SelectMany(list => list).All(str => string.IsNullOrEmpty(str.Trim()));
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

            pane_versions.Text = ui.Text("packager", "packageVersionHistory");
            pane_noItems.Text = ui.Text("packager", "packageNoItemsHeader");

            pane_uninstall.Text = ui.Text("packager", "packageUninstallHeader");
            bt_deletePackage.Text = ui.Text("packager", "packageUninstallHeader");
            bt_confirmUninstall.Text = ui.Text("packager", "packageUninstallConfirm");

            pane_uninstalled.Text = ui.Text("packager", "packageUninstalledHeader");

            var general = Panel1.NewTabPage(ui.Text("packager", "packageName"));
            general.Controls.Add(pane_meta);
            general.Controls.Add(pane_versions);


            var uninstall = Panel1.NewTabPage(ui.Text("packager", "packageUninstallHeader"));
            uninstall.Controls.Add(pane_noItems);
            uninstall.Controls.Add(pane_uninstall);
            uninstall.Controls.Add(pane_uninstalled);
        }
    }
}
