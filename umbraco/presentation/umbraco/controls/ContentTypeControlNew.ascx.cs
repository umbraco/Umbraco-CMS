using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using umbraco.cms.helpers;
using umbraco.IO;
using umbraco.presentation;
using umbraco.cms.businesslogic;

namespace umbraco.controls
{

    [ClientDependency(ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "ui/jquery.dd.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "ui/dd.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "Tree/treeIcons.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "Tree/Themes/umbraco/style.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "GenericProperty/genericproperty.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "GenericProperty/genericproperty.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "js/UmbracoCasingRules.aspx", "UmbracoRoot")]
    public partial class ContentTypeControlNew : System.Web.UI.UserControl
    {
        public uicontrols.TabPage InfoTabPage;

        // General Private members
        private cms.businesslogic.ContentType cType;
        private static string UmbracoPath = SystemDirectories.Umbraco;
        public bool HideStructure { get; set; }

        // "Tab" tab
        protected uicontrols.Pane Pane8;


        // "Structure" tab
        protected controls.DualSelectbox dualAllowedContentTypes = new DualSelectbox();

        // "Info" tab

        // "Generic properties" tab
        public uicontrols.TabPage GenericPropertiesTabPage;


        public GenericProperties.GenericPropertyWrapper gp;
        private ArrayList _genericProperties = new ArrayList();
        private ArrayList _sortLists = new ArrayList();
        protected System.Web.UI.WebControls.DataGrid dgGeneralTabProperties;

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            int docTypeId = getDocTypeId();
            cType = new cms.businesslogic.ContentType(docTypeId);

            setupInfoPane();
            if (!HideStructure)
            {
                setupStructurePane();
            }
            setupGenericPropertiesPane();
            setupTabPane();

        }

        private int getDocTypeId()
        {
            return int.Parse(Request.QueryString["id"]);
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {


            pp_newTab.Text = ui.Text("newtab", umbraco.BasePages.UmbracoEnsuredPage.CurrentUser);
            pp_alias.Text = umbraco.ui.Text("alias", umbraco.BasePages.UmbracoEnsuredPage.CurrentUser);
            pp_name.Text = umbraco.ui.Text("name", umbraco.BasePages.UmbracoEnsuredPage.CurrentUser);
            pp_allowedChildren.Text = umbraco.ui.Text("allowedchildnodetypes", umbraco.BasePages.UmbracoEnsuredPage.CurrentUser);
            pp_description.Text = umbraco.ui.Text("editcontenttype", "description");
            pp_icon.Text = umbraco.ui.Text("icon", umbraco.BasePages.UmbracoEnsuredPage.CurrentUser);
            pp_thumbnail.Text = umbraco.ui.Text("editcontenttype", "thumbnail");


            // we'll disable this...
            if (!Page.IsPostBack && cType.MasterContentType != 0)
            {
                string masterName = cms.businesslogic.ContentType.GetContentType(cType.MasterContentType).Text;
                tabsMasterContentTypeName.Text = masterName;
                propertiesMasterContentTypeName.Text = masterName;
                PaneTabsInherited.Visible = true;
                PanePropertiesInherited.Visible = true;
            }

            theClientId.Text = this.ClientID;
        }

        protected void save_click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            cType.Text = txtName.Text;
            cType.Alias = txtAlias.Text;
            cType.IconUrl = ddlIcons.SelectedValue;
            cType.Description = description.Text;
            cType.Thumbnail = ddlThumbnails.SelectedValue;
            SaveClickEventArgs ea = new SaveClickEventArgs("Saved");
            ea.IconType = umbraco.BasePages.BasePage.speechBubbleIcon.success;

            saveProperties(ref ea);

            SaveTabs();

            SaveAllowedChildTypes();

            // reload content type (due to caching)
            cType = new ContentType(cType.Id);
            bindDataGenericProperties(true);

            // we need to re-bind the alias as the SafeAlias method can have changed it
            txtAlias.Text = cType.Alias;

            RaiseBubbleEvent(new object(), ea);
        }

        #region "Info" Pane


        private void setupInfoPane()
        {
            InfoTabPage = TabView1.NewTabPage("Info");
            InfoTabPage.Controls.Add(pnlInfo);

            InfoTabPage.Style.Add("text-align", "center");

            ImageButton Save = InfoTabPage.Menu.NewImageButton();
            Save.Click += new System.Web.UI.ImageClickEventHandler(save_click);

            Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";
            Save.AlternateText = ui.Text("save");
            var listOfIcons = new List<ListItem>();
            // Get icons
            // nh css file update, add support for css sprites
            foreach (string iconClass in cms.businesslogic.CMSNode.DefaultIconClasses)
            {
                ListItem li = new ListItem(helper.SpaceCamelCasing((iconClass.Substring(1, iconClass.Length - 1))).Replace("Spr Tree", "").Trim(), iconClass);
                li.Attributes.Add("class", "spriteBackground sprTree " + iconClass.Trim('.'));
                li.Attributes.Add("style", "padding-left:20px !important; background-repeat:no-repeat;");

                if (!this.Page.IsPostBack && li.Value == cType.IconUrl) li.Selected = true;
                listOfIcons.Add(li);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(UmbracoContext.Current.Server.MapPath(SystemDirectories.Umbraco + "/images/umbraco"));
            FileInfo[] fileInfo = dirInfo.GetFiles();
            for (int i = 0; i < fileInfo.Length; i++)
            {
                // NH: don't show the sprite file
                if (fileInfo[i].Name != "sprites.png" && fileInfo[i].Name != "sprites_ie6.gif")
                {
                    ListItem li = new ListItem(fileInfo[i].Name + " (deprecated)", fileInfo[i].Name);
                    li.Attributes.Add("title", this.ResolveClientUrl(SystemDirectories.Umbraco + "/images/umbraco/" + fileInfo[i].Name));

                    if (li.Value == cType.IconUrl)
                        li.Selected = true;
                    listOfIcons.Add(li);
                }
            }

            ddlIcons.Items.AddRange(listOfIcons.OrderBy(o => o.Text).ToArray());

            // Get thumbnails
            dirInfo = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Umbraco + "/images/thumbnails"));
            fileInfo = dirInfo.GetFiles();
            for (int i = 0; i < fileInfo.Length; i++)
            {
                ListItem li = new ListItem(fileInfo[i].Name);
                li.Attributes.Add("title", this.ResolveClientUrl(SystemDirectories.Umbraco + "/images/thumbnails/" + fileInfo[i].Name));
                if (!this.Page.IsPostBack && li.Value == cType.Thumbnail) li.Selected = true;
                ddlThumbnails.Items.Add(li);
            }

            Page.ClientScript.RegisterStartupScript(this.GetType(), "thumbnailsDropDown", @"
function refreshDropDowns() {
    jQuery('#" + ddlIcons.ClientID + @"').msDropDown({ showIcon: true, style: 'width:250px;' });
    jQuery('#" + ddlThumbnails.ClientID + @"').msDropDown({ showIcon: false, rowHeight: '130', visibleRows: '2', style: 'width:250px;' });
}
jQuery(function() { refreshDropDowns(); });
", true);

            txtName.Text = cType.GetRawText();
            txtAlias.Text = cType.Alias;
            description.Text = cType.Description;

        }
        #endregion


        #region "Structure" Pane
        private void setupStructurePane()
        {
            dualAllowedContentTypes.ID = "allowedContentTypes";
            dualAllowedContentTypes.Width = 175;

            uicontrols.TabPage tp = TabView1.NewTabPage("Structure");
            tp.Controls.Add(pnlStructure);
            tp.Style.Add("text-align", "center");
            ImageButton Save = tp.Menu.NewImageButton();
            Save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
            Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";

            int[] allowedIds = cType.AllowedChildContentTypeIDs;
            if (!Page.IsPostBack)
            {
                string chosenContentTypeIDs = "";
                ContentType[] contentTypes = cType.GetAll();
                foreach (cms.businesslogic.ContentType ct in contentTypes.OrderBy(x => x.Text))
                {
                    ListItem li = new ListItem(ct.Text, ct.Id.ToString());
                    dualAllowedContentTypes.Items.Add(li);
                    lstAllowedContentTypes.Items.Add(li);
                    foreach (int i in allowedIds)
                    {
                        if (i == ct.Id)
                        {
                            li.Selected = true;
                            chosenContentTypeIDs += ct.Id + ",";
                        }
                    }
                }
                dualAllowedContentTypes.Value = chosenContentTypeIDs;
            }
        }

        private void SaveAllowedChildTypes()
        {
            ArrayList tmp = new ArrayList();
            foreach (ListItem li in lstAllowedContentTypes.Items)
            {
                if (li.Selected)
                    tmp.Add(int.Parse(li.Value));
            }
            int[] ids = new int[tmp.Count];
            for (int i = 0; i < tmp.Count; i++) ids[i] = (int)tmp[i];
            cType.AllowedChildContentTypeIDs = ids;
        }

        #endregion

        #region "Generic properties" Pane
        private void setupGenericPropertiesPane()
        {
            GenericPropertiesTabPage = TabView1.NewTabPage("Generic properties");
            GenericPropertiesTabPage.Style.Add("text-align", "center");
            GenericPropertiesTabPage.Controls.Add(pnlProperties);

            ImageButton Save = GenericPropertiesTabPage.Menu.NewImageButton();
            Save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
            Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";

            //dlTabs.ItemCommand += new DataListCommandEventHandler(dlTabs_ItemCommand);
            bindDataGenericProperties(false);
        }

        protected void dgTabs_itemdatabound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                ((DropDownList)e.Item.FindControl("dllTab")).SelectedValue =
                    ((DataRowView)e.Item.DataItem).Row["tabid"].ToString();
                ((DropDownList)e.Item.FindControl("ddlType")).SelectedValue =
                    ((DataRowView)e.Item.DataItem).Row["type"].ToString();
            }

        }
        private void bindDataGenericProperties(bool Refresh)
        {
            cms.businesslogic.ContentType.TabI[] tabs = cType.getVirtualTabs;
            cms.businesslogic.datatype.DataTypeDefinition[] dtds = cms.businesslogic.datatype.DataTypeDefinition.GetAll();

            PropertyTypes.Controls.Clear();

            // Remove any tab from list that's from a master content type (shouldn't be able to configure those from a child)
            /*            System.Collections.Generic.List<cms.businesslogic.ContentType.TabI> localTabs = new System.Collections.Generic.List<umbraco.cms.businesslogic.ContentType.TabI>();
                        foreach (cms.businesslogic.ContentType.TabI t in tabs)
                        {
                            if (t.ContentType == cType.Id)
                                localTabs.Add(t);
                        }
                        tabs = localTabs.ToArray();
            */
            // Add new property
            if (PropertyTypeNew.Controls.Count == 0)
            {
                PropertyTypeNew.Controls.Add(new LiteralControl("<h2 class=\"propertypaneTitel\">Add New Property</h2><ul class='genericPropertyList addNewProperty'>"));
                gp = new controls.GenericProperties.GenericPropertyWrapper();
                gp.ID = "GenericPropertyNew";
                gp.Tabs = tabs;
                gp.DataTypeDefinitions = dtds;
                PropertyTypeNew.Controls.Add(gp);
                PropertyTypeNew.Controls.Add(new LiteralControl("</ul>"));
            }
            else if (Refresh)
            {
                gp = (controls.GenericProperties.GenericPropertyWrapper)PropertyTypeNew.Controls[1];
                gp.ID = "GenericPropertyNew";
                gp.Tabs = tabs;
                gp.DataTypeDefinitions = dtds;
                gp.UpdateEditControl();
                gp.GenricPropertyControl.UpdateInterface();
                gp.GenricPropertyControl.Clear();
            }

            _genericProperties.Clear();
            Hashtable inTab = new Hashtable();
            int counter = 0;
            string scrollLayerId = GenericPropertiesTabPage.ClientID + "_contentlayer";

            foreach (cms.businesslogic.ContentType.TabI t in tabs)
            {
                bool hasProperties = false;
                string tabCaption = t.ContentType == cType.Id ? t.GetRawCaption() : t.GetRawCaption() + " (inherited from " + new ContentType(t.ContentType).Text + ")";
                PropertyTypes.Controls.Add(new LiteralControl("<div class='genericPropertyListBox'><h2 class=\"propertypaneTitel\">Tab: " + tabCaption + "</h2>"));

                if (t.PropertyTypes.Length > 0)
                {
                    HtmlInputHidden propSort = new HtmlInputHidden();
                    propSort.ID = "propSort_" + t.Id.ToString() + "_Content";
                    PropertyTypes.Controls.Add(propSort);
                    _sortLists.Add(propSort);

                    var pts = t.PropertyTypes.Where(pt => pt.ContentTypeId == cType.Id);

                    if (pts.Count() > 0)
                    {
                        PropertyTypes.Controls.Add(new LiteralControl("<ul class='genericPropertyList' id=\"t_" + t.Id.ToString() + "_Contents\">"));

                        foreach (cms.businesslogic.propertytype.PropertyType pt in pts)
                        {
                            GenericProperties.GenericPropertyWrapper gpw = new umbraco.controls.GenericProperties.GenericPropertyWrapper();

                            // Changed by duckie, was:
                            // gpw.ID = "gpw_" + editPropertyType.Alias;
                            // Which is NOT unique!
                            gpw.ID = "gpw_" + pt.Id;

                            gpw.PropertyType = pt;
                            gpw.Tabs = tabs;
                            gpw.TabId = t.Id;
                            gpw.DataTypeDefinitions = dtds;
                            gpw.Delete += new EventHandler(gpw_Delete);
                            gpw.FullId = "t_" + t.Id.ToString() + "_Contents_" + +pt.Id;

                            PropertyTypes.Controls.Add(gpw);
                            _genericProperties.Add(gpw);
                            if (Refresh)
                                gpw.GenricPropertyControl.UpdateInterface();
                            inTab.Add(pt.Id.ToString(), "");
                            counter++;
                            hasProperties = true;
                        }

                        PropertyTypes.Controls.Add(new LiteralControl("</ul>"));
                    }

                    var jsSortable = @"                            
                                (function($) {
                                    var propSortId = ""#" + propSort.ClientID + @""";
                                    $(document).ready(function() {
                                        $(propSortId).next("".genericPropertyList"").sortable({containment: 'parent', tolerance: 'pointer',
                                            update: function(event, ui) { 
                                                $(propSortId).val($(this).sortable('serialize'));
                                            }});
                                    });
                                })(jQuery);";

                    Page.ClientScript.RegisterStartupScript(this.GetType(), propSort.ClientID, jsSortable, true);

                    if (!hasProperties)
                    {
                        addNoPropertiesDefinedMessage();
                    }

                    PropertyTypes.Controls.Add(new LiteralControl("</div>"));
                }
                else
                {
                    addNoPropertiesDefinedMessage();
                    PropertyTypes.Controls.Add(new LiteralControl("</div>"));
                }
            }

            // Generic properties tab
            counter = 0;
            bool propertyTabHasProperties = false;
            PlaceHolder propertiesPH = new PlaceHolder();
            propertiesPH.ID = "propertiesPH";
            PropertyTypes.Controls.Add(new LiteralControl("<h2 class=\"propertypaneTitel\">Tab: Generic Properties</h2>"));
            PropertyTypes.Controls.Add(propertiesPH);

            HtmlInputHidden propSort_gp = new HtmlInputHidden();
            propSort_gp.ID = "propSort_general_Content";
            propertiesPH.Controls.Add(propSort_gp);
            _sortLists.Add(propSort_gp);


            propertiesPH.Controls.Add(new LiteralControl("<ul class='genericPropertyList' id=\"t_general_Contents\">"));
            foreach (cms.businesslogic.propertytype.PropertyType pt in cType.PropertyTypes)
            {
                //This use to be:
                //if (pt.ContentTypeId == cType.Id && !inTab.ContainsKey(pt.Id.ToString())
                //But seriously, if it's not on a tab the tabId is 0, it's a lot easier to read IMO
                if (pt.ContentTypeId == cType.Id && pt.TabId == 0)
                {
                    GenericProperties.GenericPropertyWrapper gpw = new umbraco.controls.GenericProperties.GenericPropertyWrapper();

                    // Changed by duckie, was:
                    // gpw.ID = "gpw_" + editPropertyType.Alias;
                    // Which is NOT unique!
                    gpw.ID = "gpw_" + pt.Id;

                    gpw.PropertyType = pt;
                    gpw.Tabs = tabs;
                    gpw.DataTypeDefinitions = dtds;
                    gpw.Delete += new EventHandler(gpw_Delete);
                    gpw.FullId = "t_general_Contents_" + pt.Id;

                    propertiesPH.Controls.Add(gpw);
                    _genericProperties.Add(gpw);
                    if (Refresh)
                        gpw.GenricPropertyControl.UpdateInterface();
                    inTab.Add(pt.Id, "");
                    propertyTabHasProperties = true;
                    counter++;
                }
            }


            propertiesPH.Controls.Add(new LiteralControl("</ul>"));
            //propertiesPH.Controls.Add(new LiteralControl("<script>\n Sortable.create(\"generalPropertiesContents\",{scroll: '" + scrollLayerId + "',dropOnEmpty:false,containment:[\"generalPropertiesContents\"],constraint:'vertical',onUpdate:function(element) {document.getElementById('" + propSort_gp.ClientID + "').value = Sortable.serialize('generalPropertiesContents');}});\n</script>"));

            var jsSortable_gp = @"                
                    (function($) {
                        var propSortId = ""#" + propSort_gp.ClientID + @""";
                        $(document).ready(function() {
                            $(propSortId).next("".genericPropertyList"").sortable({containment: 'parent', tolerance: 'pointer',
                                update: function(event, ui) { 
                                    $(propSortId).val($(this).sortable('serialize'));
                                }});
                        });
                    })(jQuery);";

            Page.ClientScript.RegisterStartupScript(this.GetType(), "propSort_gp", jsSortable_gp, true);


            if (!propertyTabHasProperties)
            {
                PropertyTypes.Controls.Add(new LiteralControl("<div style=\"margin: 10px; padding: 4px; border: 1px solid #ccc;\">No properties defined on this tab. Click on the \"add a new property\" link at the top to create a new property.</div>"));
                PropertyTypes.Controls.Remove(PropertyTypes.FindControl("propertiesPH"));
            }
            else
                PropertyTypes.Controls.Add(propertiesPH);

        }

        private void addNoPropertiesDefinedMessage()
        {
            PropertyTypes.Controls.Add(new LiteralControl("<div style=\"margin: 10px; padding: 4px; border: 1px solid #ccc;\">No properties defined on this tab. Click on the \"add a new property\" link at the top to create a new property.</div>"));
        }

        protected void gpw_Delete(object sender, System.EventArgs e)
        {
            GenericProperties.GenericPropertyWrapper gpw = (GenericProperties.GenericPropertyWrapper)sender;
            gpw.GenricPropertyControl.PropertyType.delete();
            cType = ContentType.GetContentType(cType.Id);
            this.bindDataGenericProperties(true);
        }


        private void _old_bindDataGenericProperties()
        {

            // Data bind create new
            gp.GenricPropertyControl.Tabs = cType.getVirtualTabs;
            gp.GenricPropertyControl.DataTypeDefinitions = cms.businesslogic.datatype.DataTypeDefinition.GetAll();

            DataSet ds = new DataSet();

            DataTable dtP = new DataTable("Properties");
            DataTable dtT = new DataTable("Tabs");

            ds.Tables.Add(dtP);
            ds.Tables.Add(dtT);

            dtP.Columns.Add("id");
            dtP.Columns.Add("tabid");
            dtP.Columns.Add("alias");
            dtP.Columns.Add("name");
            dtP.Columns.Add("type");
            dtP.Columns.Add("tab");

            dtT.Columns.Add("tabid");
            dtT.Columns.Add("TabName");
            dtT.Columns.Add("genericProperties");

            Hashtable inTab = new Hashtable();
            foreach (cms.businesslogic.ContentType.TabI tb in cType.getVirtualTabs.ToList())
            {
                DataRow dr = dtT.NewRow();
                dr["TabName"] = tb.GetRawCaption();
                dr["tabid"] = tb.Id;
                dtT.Rows.Add(dr);

                foreach (cms.businesslogic.propertytype.PropertyType pt in tb.PropertyTypes)
                {
                    DataRow dr1 = dtP.NewRow();
                    dr1["alias"] = pt.Alias;
                    dr1["tabid"] = tb.Id;
                    dr1["name"] = pt.GetRawName();
                    dr1["type"] = pt.DataTypeDefinition.Id;
                    dr1["tab"] = tb.GetRawCaption();
                    dr1["id"] = pt.Id.ToString();
                    dtP.Rows.Add(dr1);
                    inTab.Add(pt.Id.ToString(), true);
                }
            }

            DataRow dr2 = dtT.NewRow();
            dr2["TabName"] = "General properties";
            dr2["tabid"] = 0;
            dtT.Rows.Add(dr2);

            foreach (cms.businesslogic.propertytype.PropertyType pt in cType.PropertyTypes)
            {
                if (!inTab.ContainsKey(pt.Id.ToString()))
                {
                    DataRow dr1 = dtP.NewRow();
                    dr1["alias"] = pt.Alias;
                    dr1["tabid"] = 0;
                    dr1["name"] = pt.GetRawName();
                    dr1["type"] = pt.DataTypeDefinition.Id;
                    dr1["tab"] = "General properties";
                    dr1["id"] = pt.Id.ToString();
                    dtP.Rows.Add(dr1);
                }
            }


            ds.Relations.Add(new DataRelation("tabidrelation", dtT.Columns["tabid"], dtP.Columns["tabid"], false));
        }


        private void saveProperties(ref SaveClickEventArgs e)
        {
            this.CreateChildControls();

            GenericProperties.GenericProperty gpData = gp.GenricPropertyControl;
            if (gpData.Name.Trim() != "" && gpData.Alias.Trim() != "")
            {
                if (cType.getPropertyType(Casing.SafeAliasWithForcingCheck(gpData.Alias.Trim())) == null)
                {
                    string[] info = { gpData.Name, gpData.Type.ToString() };
                    cms.businesslogic.propertytype.PropertyType pt = cType.AddPropertyType(cms.businesslogic.datatype.DataTypeDefinition.GetDataTypeDefinition(gpData.Type), Casing.SafeAliasWithForcingCheck(gpData.Alias.Trim()), gpData.Name);
                    pt.Mandatory = gpData.Mandatory;
                    pt.ValidationRegExp = gpData.Validation;
                    pt.Description = gpData.Description;

                    if (gpData.Tab != 0)
                    {
                        cType.SetTabOnPropertyType(pt, gpData.Tab);
                    }

                    gpData.Clear();

                }
                else
                {
                    e.Message = ui.Text("contentTypeDublicatePropertyType");
                    e.IconType = umbraco.BasePages.BasePage.speechBubbleIcon.warning;
                }
            }

            foreach (GenericProperties.GenericPropertyWrapper gpw in _genericProperties)
            {
                cms.businesslogic.propertytype.PropertyType pt = gpw.PropertyType;
                pt.Alias = gpw.GenricPropertyControl.Alias;
                pt.Name = gpw.GenricPropertyControl.Name;
                pt.Description = gpw.GenricPropertyControl.Description;
                pt.ValidationRegExp = gpw.GenricPropertyControl.Validation;
                pt.Mandatory = gpw.GenricPropertyControl.Mandatory;
                pt.DataTypeDefinition = cms.businesslogic.datatype.DataTypeDefinition.GetDataTypeDefinition(gpw.GenricPropertyControl.Type);
                if (gpw.GenricPropertyControl.Tab == 0)
                    cType.removePropertyTypeFromTab(pt);
                else
                    cType.SetTabOnPropertyType(pt, gpw.GenricPropertyControl.Tab);

                pt.Save();
            }

            // Sort order
            foreach (HtmlInputHidden propSorter in _sortLists)
            {
                if (propSorter.Value.Trim() != "")
                {
                    string tabId = propSorter.ID;
                    // remove leading "propSort_" and trailing "_Content"
                    tabId = tabId.Substring(9, tabId.Length - 9 - 8);
                    // calc the position of the prop SO i.e. after "t_<tabId>Contents[]="
                    int propSOPosition = "t_".Length + tabId.Length + "Contents[]=".Length + 1;

                    string[] tempSO = propSorter.Value.Split("&".ToCharArray());
                    for (int i = 0; i < tempSO.Length; i++)
                    {
                        string propSO = tempSO[i].Substring(propSOPosition);
                        int currentSortOrder = int.Parse(propSO);
                        cms.businesslogic.propertytype.PropertyType.GetPropertyType(currentSortOrder).SortOrder = i;
                    }
                }
            }
        }

        public bool HasRows(System.Data.DataView dv)
        {
            return (dv.Count == 0);
        }
        private void dlTabs_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                cType.DeleteVirtualTab(int.Parse(e.CommandArgument.ToString()));
            }

            if (e.CommandName == "MoveDown")
            {
                int TabId = int.Parse(e.CommandArgument.ToString());
                foreach (cms.businesslogic.ContentType.TabI t in cType.getVirtualTabs.ToList())
                {
                    if (t.Id == TabId)
                    {
                        t.MoveDown();
                    }
                }
            }

            if (e.CommandName == "MoveUp")
            {
                int TabId = int.Parse(e.CommandArgument.ToString());
                foreach (cms.businesslogic.ContentType.TabI t in cType.getVirtualTabs.ToList())
                {
                    if (t.Id == TabId)
                    {
                        t.MoveUp();
                    }
                }
            }
            bindTabs();
            bindDataGenericProperties(false);
        }


        protected void dgGenericPropertiesOfTab_itemcommand(object sender, DataGridCommandEventArgs e)
        {
            // Delete propertytype from contenttype
            if (e.CommandName == "Delete")
            {
                int propertyId = int.Parse(e.Item.Cells[0].Text);
                cms.businesslogic.propertytype.PropertyType pt = cms.businesslogic.propertytype.PropertyType.GetPropertyType(propertyId);
                RaiseBubbleEvent(new object(), new SaveClickEventArgs("Property ´" + pt.GetRawName() + "´ deleted"));
                pt.delete();
                bindDataGenericProperties(false);
            }
        }

        protected void dlTab_itemdatabound(object sender, DataListItemEventArgs e)
        {
            if (int.Parse(((DataRowView)e.Item.DataItem).Row["tabid"].ToString()) == 0)
            {
                ((Button)e.Item.FindControl("btnTabDelete")).Visible = false;
                ((Button)e.Item.FindControl("btnTabUp")).Visible = false;
                ((Button)e.Item.FindControl("btnTabDown")).Visible = false;
            }
        }


        #endregion

        #region "Tab" Pane
        private void setupTabPane()
        {
            uicontrols.TabPage tp = TabView1.NewTabPage("Tabs");


            pnlTab.Style.Add("text-align", "center");
            tp.Controls.Add(pnlTab);
            ImageButton Save = tp.Menu.NewImageButton();
            Save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
            Save.ID = "SaveButton";
            Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";
            bindTabs();
        }

        private void bindTabs()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("name");
            dt.Columns.Add("id");
            dt.Columns.Add("order");
            foreach (cms.businesslogic.ContentType.TabI tb in cType.getVirtualTabs.ToList())
            {
                if (tb.ContentType == cType.Id)
                {
                    DataRow dr = dt.NewRow();
                    dr["name"] = tb.GetRawCaption();
                    dr["id"] = tb.Id;
                    dr["order"] = tb.SortOrder;
                    dt.Rows.Add(dr);
                }
            }

            if (dt.Rows.Count == 0)
            {
                lttNoTabs.Text = "No custom tabs defined";
                dgTabs.Visible = false;
            }
            else
            {
                lttNoTabs.Text = "";
                dgTabs.Visible = true;
            }
            dgTabs.DataSource = dt;
            dgTabs.DataBind();
        }

        public DataTable TabTable
        {
            get
            {
                if (dgTabs.DataSource == null)
                    bindTabs();

                DataTable dt = new DataTable();
                dt.Columns.Add("name");
                dt.Columns.Add("id");

                foreach (DataRow dr in ((DataTable)dgTabs.DataSource).Rows)
                {
                    DataRow dr2 = dt.NewRow();
                    dr2["name"] = dr["name"];
                    dr2["id"] = dr["id"];
                    dt.Rows.Add(dr2);
                }

                DataRow dr1 = dt.NewRow();
                dr1["name"] = "General properties";
                dr1["id"] = 0;
                dt.Rows.Add(dr1);

                return dt;
            }
        }

        private DataTable _DataTypeTable;

        public DataTable DataTypeTable
        {
            get
            {
                if (_DataTypeTable == null)
                {
                    _DataTypeTable = new DataTable();
                    _DataTypeTable.Columns.Add("name");
                    _DataTypeTable.Columns.Add("id");
                    foreach (cms.businesslogic.datatype.DataTypeDefinition DataType in cms.businesslogic.datatype.DataTypeDefinition.GetAll())
                    {
                        DataRow dr = _DataTypeTable.NewRow();
                        dr["name"] = DataType.Text;
                        dr["id"] = DataType.Id.ToString();
                        _DataTypeTable.Rows.Add(dr);
                    }
                }
                return _DataTypeTable;
            }
        }


        protected void btnNewTab_Click(object sender, System.EventArgs e)
        {
            if (txtNewTab.Text.Trim() != "")
            {
                cType.AddVirtualTab(txtNewTab.Text);
                SaveClickEventArgs ea = new SaveClickEventArgs(ui.Text("contentTypeTabCreated"));
                ea.IconType = umbraco.BasePages.BasePage.speechBubbleIcon.success;

                RaiseBubbleEvent(new object(), ea);

                txtNewTab.Text = "";
                bindTabs();
                bindDataGenericProperties(true);
            }

            Page.ClientScript.RegisterStartupScript(this.GetType(), "dropDowns", @"
Umbraco.Controls.TabView.onActiveTabChange(function(tabviewid, tabid, tabs) {
    refreshDropDowns();
});
", true);
        }

        protected void dgTabs_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                cType.DeleteVirtualTab(int.Parse(e.Item.Cells[0].Text));

                SaveClickEventArgs ea = new SaveClickEventArgs(ui.Text("contentTypeTabDeleted"));
                ea.IconType = umbraco.BasePages.BasePage.speechBubbleIcon.success;

                RaiseBubbleEvent(new object(), ea);

            }


            bindTabs();
            bindDataGenericProperties(true);
        }


        private void SaveTabs()
        {
            int tabid;
            string tabName;
            int tabSortOrder;
            foreach (DataGridItem dgi in dgTabs.Items)
            {
                tabid = int.Parse(dgi.Cells[0].Text);
                tabName = ((TextBox)dgi.FindControl("txtTab")).Text.Replace("'", "''");
                cType.SetTabName(tabid, tabName);
                if (Int32.TryParse(((TextBox)dgi.FindControl("txtSortOrder")).Text, out tabSortOrder))
                {
                    cType.SetTabSortOrder(tabid, tabSortOrder);
                }
            }
        }

        #endregion



    }
}