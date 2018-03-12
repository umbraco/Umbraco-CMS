using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Xml;

using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation.Trees;
using umbraco.controls;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace umbraco.presentation.developer.packages
{
    public partial class _Default : BasePages.UmbracoEnsuredPage
    {

        public _Default()
        {
            CurrentApp = BusinessLogic.DefaultApps.developer.ToString();

        }
        public uicontrols.TabPage packageInfo;
        public uicontrols.TabPage packageContents;
        public uicontrols.TabPage packageFiles;
        public uicontrols.TabPage packageOutput;
        public uicontrols.TabPage packageAbout;
        public uicontrols.TabPage packageActions;

        protected ContentPicker cp;
        private cms.businesslogic.packager.PackageInstance pack;
        private cms.businesslogic.packager.CreatedPackage createdPackage;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["id"] != null)
            {
                createdPackage = cms.businesslogic.packager.CreatedPackage.GetById(int.Parse(Request.QueryString["id"]));
                pack = createdPackage.Data;

                /* CONTENT */

                cp = new ContentPicker();
                content.Controls.Add(cp);
                
                if (string.IsNullOrEmpty(pack.PackagePath) == false)
                {
                    packageUmbFile.Text = " &nbsp; <a href='" + Page.ResolveClientUrl(pack.PackagePath) + "'>Download</a>";                    
                }
                else
                {
                    packageUmbFile.Text = "<em>This package is not published</em>";
                }

                if (Page.IsPostBack == false)
                {
                    ClientTools
                        .SetActiveTreeType(Constants.Trees.Packages)
                        .SyncTree("-1,created," + createdPackage.Data.Id, false);

                    packageAuthorName.Text = pack.Author;
                    packageAuthorUrl.Text = pack.AuthorUrl;
                    packageLicenseName.Text = pack.License;
                    packageLicenseUrl.Text = pack.LicenseUrl;
                    packageName.Text = pack.Name;
                    packageReadme.Text = pack.Readme;
                    packageVersion.Text = pack.Version;
                    packageUrl.Text = pack.Url;
                    iconUrl.Text = pack.IconUrl;
                    umbracoVersion.Text = pack.UmbracoVersion != null ? pack.UmbracoVersion.ToString(3) : string.Empty;

                    /*ACTIONS XML*/
                    tb_actions.Text = pack.Actions;

                    cp.Value = pack.ContentNodeId.ToString();

                    //startNode.Value = pack.ContentNodeId.ToString();

                    packageContentSubdirs.Checked = pack.ContentLoadChildNodes;


                    /*TEMPLATES */
                    Template[] umbTemplates = Template.GetAllAsList().ToArray();
                    foreach (Template tmp in umbTemplates)
                    {
                        ListItem li = new ListItem(tmp.Text, tmp.Id.ToString());

                        if (pack.Templates.Contains(tmp.Id.ToString()))
                            li.Selected = true;

                        templates.Items.Add(li);
                    }

                    /* DOC TYPES */
                    DocumentType[] docs = DocumentType.GetAllAsList().ToArray();
                    foreach (DocumentType dc in docs)
                    {
                        ListItem li = new ListItem(dc.Text, dc.Id.ToString());
                        if (pack.Documenttypes.Contains(dc.Id.ToString()))
                            li.Selected = true;

                        documentTypes.Items.Add(li);
                    }

                    /*Stylesheets */
                    var sheets = Services.FileService.GetStylesheets();
                    foreach (var st in sheets)
                    {
                        if (string.IsNullOrEmpty(st.Name) == false)
                        {
                            var li = new ListItem(st.Alias, st.Name);
                            if (pack.Stylesheets.Contains(st.Name))
                                li.Selected = true;
                            stylesheets.Items.Add(li);
                        }
                    }

                    /* MACROS */
                    Macro[] umbMacros = Macro.GetAll();
                    foreach (Macro m in umbMacros)
                    {
                        ListItem li = new ListItem(m.Name, m.Id.ToString());
                        if (pack.Macros.Contains(m.Id.ToString()))
                            li.Selected = true;

                        macros.Items.Add(li);
                    }

                    /*Langauges */
                    Language[] umbLanguages = Language.getAll;
                    foreach (Language l in umbLanguages)
                    {
                        ListItem li = new ListItem(l.FriendlyName, l.id.ToString());
                        if (pack.Languages.Contains(l.id.ToString()))
                            li.Selected = true;

                        languages.Items.Add(li);
                    }

                    /*Dictionary Items*/
                    Dictionary.DictionaryItem[] umbDictionary = Dictionary.getTopMostItems;
                    foreach (Dictionary.DictionaryItem d in umbDictionary)
                    {

                        string liName = d.key;
                        if (d.hasChildren)
                            liName += " <small>(Including all child items)</small>";

                        ListItem li = new ListItem(liName, d.id.ToString());

                        if (pack.DictionaryItems.Contains(d.id.ToString()))
                            li.Selected = true;

                        dictionary.Items.Add(li);
                    }

                    /*Data types */
                    cms.businesslogic.datatype.DataTypeDefinition[] umbDataType = cms.businesslogic.datatype.DataTypeDefinition.GetAll();

                    // sort array by name
                    Array.Sort(umbDataType, delegate(cms.businesslogic.datatype.DataTypeDefinition umbDataType1, cms.businesslogic.datatype.DataTypeDefinition umbDataType2)
                    {
                        return umbDataType1.Text.CompareTo(umbDataType2.Text);
                    });

                    foreach (cms.businesslogic.datatype.DataTypeDefinition umbDtd in umbDataType)
                    {

                        ListItem li = new ListItem(umbDtd.Text, umbDtd.Id.ToString());

                        if (pack.DataTypes.Contains(umbDtd.Id.ToString()))
                            li.Selected = true;

                        cbl_datatypes.Items.Add(li);
                    }

                    /* FILES */
                    packageFilesRepeater.DataSource = pack.Files;
                    packageFilesRepeater.DataBind();

                    packageControlPath.Text = pack.LoadControl;
                }
                else
                {
                    ClientTools
                        .SetActiveTreeType(Constants.Trees.Packages)
                        .SyncTree("-1,created," + createdPackage.Data.Id, true);
                }
            }
        }

        protected void validateActions(object sender, ServerValidateEventArgs e)
        {
            string actions = tb_actions.Text;
            if (!string.IsNullOrEmpty(actions))
            {

                actions = "<Actions>" + actions + "</Actions>";

                try
                {
                    //we try to load an xml document with the potential malformed xml to ensure that this is actual action xml... 
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml(actions);
                    e.IsValid = true;
                }
                catch
                {
                    e.IsValid = false;
                }
            }
            else
                e.IsValid = true;
        }

        protected void saveOrPublish(object sender, CommandEventArgs e)
        {

            if (!Page.IsValid)
            {
                this.ClientTools.ShowSpeechBubble(BasePages.BasePage.speechBubbleIcon.error, "Saved failed.", "Some fields have not been filled-out correctly");
            }
            else
            {
                if (e.CommandName == "save")
                    SavePackage(true);

                if (e.CommandName == "publish")
                {
                    SavePackage(false);
                    int packageID = int.Parse(Request.QueryString["id"]);
                    //string packFileName = cms.businesslogic.packager. Publish.publishPackage(packageID);

                    createdPackage.Publish();


                    if (!string.IsNullOrEmpty(pack.PackagePath))
                    {

                        packageUmbFile.Text = " &nbsp; <a href='" + IOHelper.ResolveUrl(pack.PackagePath) + "'>Download</a>";

                        this.ClientTools.ShowSpeechBubble(BasePages.BasePage.speechBubbleIcon.success, "Package saved and published", "");
                    }
                    else
                    {
                        this.ClientTools.ShowSpeechBubble(BasePages.BasePage.speechBubbleIcon.error, "Save failed", "check your umbraco log.");
                    }
                }
            }
        }

        [Obsolete("This is not used")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void generateXML(object sender, EventArgs e)
        {
        }

        private void SavePackage(bool showNotification)
        {
            pack.Author = packageAuthorName.Text;
            pack.AuthorUrl = packageAuthorUrl.Text;

            pack.License = packageLicenseName.Text;
            pack.LicenseUrl = packageLicenseUrl.Text;

            pack.Readme = packageReadme.Text;
            pack.Actions = tb_actions.Text;

            pack.Name = packageName.Text;
            pack.Url = packageUrl.Text;
            pack.Version = packageVersion.Text;
            pack.IconUrl = iconUrl.Text;
            pack.UmbracoVersion = Version.Parse(umbracoVersion.Text);

            pack.ContentLoadChildNodes = packageContentSubdirs.Checked;

            if (string.IsNullOrEmpty(cp.Value) == false)
                pack.ContentNodeId = cp.Value;
            else
                pack.ContentNodeId = "";


            string tmpStylesheets = "";
            foreach (ListItem li in stylesheets.Items)
            {
                if (li.Selected)
                    tmpStylesheets += li.Value + ",";
            }
            pack.Stylesheets = new List<string>(tmpStylesheets.Trim(',').Split(','));


            string tmpDoctypes = "";
            foreach (ListItem li in documentTypes.Items)
            {
                if (li.Selected)
                    tmpDoctypes += li.Value + ",";
            }
            pack.Documenttypes = new List<string>(tmpDoctypes.Trim(',').Split(','));


            string tmpMacros = "";
            foreach (ListItem li in macros.Items)
            {
                if (li.Selected)
                    tmpMacros += li.Value + ",";
            }
            pack.Macros = new List<string>(tmpMacros.Trim(',').Split(','));


            string tmpLanguages = "";
            foreach (ListItem li in languages.Items)
            {
                if (li.Selected)
                    tmpLanguages += li.Value + ",";
            }
            pack.Languages = new List<string>(tmpLanguages.Trim(',').Split(','));

            string tmpDictionaries = "";
            foreach (ListItem li in dictionary.Items)
            {
                if (li.Selected)
                    tmpDictionaries += li.Value + ",";
            }
            pack.DictionaryItems = new List<string>(tmpDictionaries.Trim(',').Split(','));


            string tmpTemplates = "";
            foreach (ListItem li in templates.Items)
            {
                if (li.Selected)
                    tmpTemplates += li.Value + ",";
            }
            pack.Templates = new List<string>(tmpTemplates.Trim(',').Split(','));

            string tmpDataTypes = "";
            foreach (ListItem li in cbl_datatypes.Items)
            {
                if (li.Selected)
                    tmpDataTypes += li.Value + ",";
            }
            pack.DataTypes = new List<string>(tmpDataTypes.Trim(',').Split(','));

            pack.LoadControl = packageControlPath.Text;


            createdPackage.Save();

            if (showNotification)
                this.ClientTools.ShowSpeechBubble(BasePages.BasePage.speechBubbleIcon.save, "Package Saved", "");
        }

        protected void addFileToPackage(object sender, EventArgs e)
        {
            string newPath = packageFilePathNew.Text;

            if (newPath.Trim() != "")
            {
                cms.businesslogic.packager.CreatedPackage createdPackage = cms.businesslogic.packager.CreatedPackage.GetById(int.Parse(Request.QueryString["id"]));
                cms.businesslogic.packager.PackageInstance pack = createdPackage.Data;

                pack.Files.Add(newPath);

                createdPackage.Save();

                packageFilePathNew.Text = "";

                packageFilesRepeater.DataSource = pack.Files;
                packageFilesRepeater.DataBind();
            }
        }

        protected void deleteFileFromPackage(object sender, EventArgs e)
        {
            TextBox filePathControl = (TextBox)((Control)sender).Parent.FindControl("packageFilePath");
            filePathControl.Text = "";

            string tmpFilePathString = "";
            foreach (RepeaterItem rItem in packageFilesRepeater.Items)
            {
                string tmpFFFF = ((TextBox)rItem.FindControl("packageFilePath")).Text;
                if (tmpFFFF.Trim() != "")
                    tmpFilePathString += tmpFFFF + "¤";
            }

            cms.businesslogic.packager.CreatedPackage createdPackage = cms.businesslogic.packager.CreatedPackage.GetById(int.Parse(Request.QueryString["id"]));
            cms.businesslogic.packager.PackageInstance pack = createdPackage.Data;

            pack.Files = new List<string>(tmpFilePathString.Trim('¤').Split('¤'));
            pack.Files.TrimExcess();

            createdPackage.Save();

            packageFilesRepeater.DataSource = pack.Files;
            packageFilesRepeater.DataBind();
        }

        protected override void OnInit(EventArgs e)
        {
            // Tab setup
            packageInfo = TabView1.NewTabPage("Package Properties");
            packageInfo.Controls.Add(Pane1);
            packageInfo.Controls.Add(Pane5);
            packageInfo.Controls.Add(Pane1_1);
            packageInfo.Controls.Add(Pane1_2);
            packageInfo.Controls.Add(Pane1_3);


            packageContents = TabView1.NewTabPage("Package Contents");
            packageContents.Controls.Add(Pane2);
            packageContents.Controls.Add(Pane2_1);
            packageContents.Controls.Add(Pane2_2);
            packageContents.Controls.Add(Pane2_3);
            packageContents.Controls.Add(Pane2_4);
            packageContents.Controls.Add(Pane2_5);
            packageContents.Controls.Add(Pane2_6);
            packageContents.Controls.Add(Pane2_7);

            packageFiles = TabView1.NewTabPage("Package Files");
            packageFiles.Controls.Add(Pane3);
            packageFiles.Controls.Add(Pane3_1);
            packageFiles.Controls.Add(Pane3_2);

            packageActions = TabView1.NewTabPage("Package Actions");
            packageActions.Controls.Add(Pane4);

            var pubs = TabView1.Menu.NewButton();
            pubs.Text = ui.Text("publish");
            pubs.CommandName = "publish";
            pubs.Command += new CommandEventHandler(saveOrPublish);
            pubs.ID = "saveAndPublish";

            var saves = TabView1.Menu.NewButton();
            saves.Text = ui.Text("save");
            saves.CommandName = "save";
            saves.Command += new CommandEventHandler(saveOrPublish);
            saves.ButtonType = uicontrols.MenuButtonType.Primary;
            saves.ID = "save";




            base.OnInit(e);
        }

    }
}
