using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.helpers;
using umbraco.cms.presentation.Trees;
using umbraco.controls.GenericProperties;
using Umbraco.Core.IO;
using umbraco.presentation;
using umbraco.cms.businesslogic;
using umbraco.BasePages;
using Tuple = System.Tuple;

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
        private cms.businesslogic.ContentType _contentType;
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

        //the async saving task
        private Action<SaveAsyncState> _asyncSaveTask;

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            LoadContentType();

            SetupInfoPane();
            if (!HideStructure)
            {
                SetupStructurePane();
            }
            SetupGenericPropertiesPane();
            SetupTabPane();

        }        

        protected void Page_Load(object sender, System.EventArgs e)
        {
            pp_newTab.Text = ui.Text("newtab", CurrentUser);
            pp_alias.Text = umbraco.ui.Text("alias", CurrentUser);
            pp_name.Text = umbraco.ui.Text("name", CurrentUser);
            pp_allowedChildren.Text = umbraco.ui.Text("allowedchildnodetypes", CurrentUser);
            pp_description.Text = umbraco.ui.Text("editcontenttype", "description", CurrentUser);
            pp_icon.Text = umbraco.ui.Text("icon", CurrentUser);
            pp_thumbnail.Text = umbraco.ui.Text("editcontenttype", "thumbnail", CurrentUser);


            // we'll disable this...
            if (!Page.IsPostBack && _contentType.MasterContentType != 0)
            {
                string masterName = cms.businesslogic.ContentType.GetContentType(_contentType.MasterContentType).Text;
                tabsMasterContentTypeName.Text = masterName;
                propertiesMasterContentTypeName.Text = masterName;
                PaneTabsInherited.Visible = true;
                PanePropertiesInherited.Visible = true;
            }

            theClientId.Text = this.ClientID;
        }

        //SD: this is temporary in v4, in v6 we have a proper user control hierarchy
        //containing this property.
        //this is required due to this issue: http://issues.umbraco.org/issue/u4-493
        //because we need to execute some code in async but due to the localization 
        //framework requiring an httpcontext.current, it will not work. 
        //http://issues.umbraco.org/issue/u4-2143
        //so, we are going to make a property here and ensure that the basepage has
        //resolved the user before we execute the async task so that in this method
        //our calls to ui.text will include the current user and not rely on the 
        //httpcontext.current. This also improves performance:
        // http://issues.umbraco.org/issue/U4-2142
        private User CurrentUser
        {
            get { return ((BasePage)Page).getUser(); }
        }

        /// <summary>
        /// A class to track the async state for saving the doc type
        /// </summary>
        private class SaveAsyncState
        {
            public SaveAsyncState(
                SaveClickEventArgs saveArgs, 
                string originalAlias, 
                string originalName,
                string[] originalPropertyAliases)
            {
                SaveArgs = saveArgs;
                _originalAlias = originalAlias;
                _originalName = originalName;
                _originalPropertyAliases = originalPropertyAliases;
            }

            public SaveClickEventArgs SaveArgs { get; private set; }
            private readonly string _originalAlias;
            private readonly string _originalName;
            private readonly string[] _originalPropertyAliases;

            public bool HasAliasChanged(ContentType contentType)
            {
                return (string.Compare(_originalAlias, contentType.Alias, StringComparison.OrdinalIgnoreCase) != 0);
            }
            public bool HasNameChanged(ContentType contentType)
            {
                return (string.Compare(_originalName, contentType.Text, StringComparison.OrdinalIgnoreCase) != 0);
            }

            /// <summary>
            /// Returns true if any property has been removed or if any alias has changed
            /// </summary>
            /// <param name="contentType"></param>
            /// <returns></returns>
            public bool HasAnyPropertyAliasChanged(ContentType contentType)
            {                
                var newAliases = contentType.PropertyTypes.Select(x => x.Alias).ToArray();
                //if any have been removed, return true
                if (newAliases.Length < _originalPropertyAliases.Count())
                {
                    return true;
                }
                //otherwise ensure that all of the original aliases are still existing
                return newAliases.ContainsAll(_originalPropertyAliases) == false;
            }
        }

        /// <summary>
        /// Called asynchronously in order to persist all of the data to the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="cb"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <remarks>
        /// This can be a long running operation depending on how many content nodes exist and if the node type alias
        /// has changed as this will need to regenerate XML for all of the nodes.
        /// </remarks>
        private IAsyncResult BeginAsyncSaveOperation(object sender, EventArgs e, AsyncCallback cb, object state)
        {
            Trace.Write("ContentTypeControlNew", "Start async operation");

            //get the args from the async state
            var args = (SaveAsyncState)state;

            //start the task
            var result = _asyncSaveTask.BeginInvoke(args, cb, args);
            return result;
        }

        /// <summary>
        /// Occurs once the async database save operation has completed
        /// </summary>
        /// <param name="ar"></param>
        /// <remarks>
        /// This updates the UI elements
        /// </remarks>
        private void EndAsyncSaveOperation(IAsyncResult ar)
        {
            Trace.Write("ContentTypeControlNew", "ending async operation");
            
            //get the args from the async state
            var state = (SaveAsyncState)ar.AsyncState;

            // reload content type (due to caching)
            LoadContentType();
            BindDataGenericProperties(true);

            // we need to re-bind the alias as the SafeAlias method can have changed it
            txtAlias.Text = _contentType.Alias;

            RaiseBubbleEvent(new object(), state.SaveArgs);

            if (state.HasNameChanged(_contentType))
                UpdateTreeNode();

            Trace.Write("ContentTypeControlNew", "async operation ended");

            //complete it
            _asyncSaveTask.EndInvoke(ar);
        }

        private void HandleAsyncSaveTimeout(IAsyncResult ar)
        {
            Trace.Write("ContentTypeControlNew", "async operation timed out!");

            LogHelper.Error<ContentTypeControlNew>(
                "The content type saving operation timed out",
                new TimeoutException("The content type saving operation timed out. This could cause problems because the xml for the content node might not have been generated. "));

        }

        /// <summary>
        /// The save button click event handlers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void save_click(object sender, System.Web.UI.ImageClickEventArgs e)
        {

            var state = new SaveAsyncState(new SaveClickEventArgs("Saved")
                {
                    IconType = BasePage.speechBubbleIcon.success
                }, _contentType.Alias, _contentType.Text, _contentType.PropertyTypes.Select(x => x.Alias).ToArray());

            //Add the async operation to the page
            Page.RegisterAsyncTask(new PageAsyncTask(BeginAsyncSaveOperation, EndAsyncSaveOperation, HandleAsyncSaveTimeout, state));
            
            //create the save task to be executed async
            _asyncSaveTask = asyncState =>
                {
                    Trace.Write("ContentTypeControlNew", "executing task");

                    _contentType.Text = txtName.Text;
                    _contentType.Alias = txtAlias.Text;
                    _contentType.IconUrl = ddlIcons.SelectedValue;
                    _contentType.Description = description.Text;
                    _contentType.Thumbnail = ddlThumbnails.SelectedValue;

                    SaveProperties(asyncState.SaveArgs);

                    SaveTabs();

                    SaveAllowedChildTypes();
                    
                    // Only if the doctype alias changed, cause a regeneration of the xml cache file since
                    // the xml element names will need to be updated to reflect the new alias
                    if (asyncState.HasAliasChanged(_contentType) || asyncState.HasAnyPropertyAliasChanged(_contentType))
                        RegenerateXmlCaches();

                    Trace.Write("ContentTypeControlNew", "task completing");
                };

            //execute the async tasks
            Page.ExecuteRegisteredAsyncTasks();
        }

        /// <summary>
        /// Loads the current ContentType from the id found in the querystring.
        /// The correct type is loaded based on editing location (DocumentType, MediaType or MemberType).
        /// </summary>
        private void LoadContentType()
        {
            int docTypeId = int.Parse(Request.QueryString["id"]);
            LoadContentType(docTypeId);
        }

        private void LoadContentType(int docTypeId)
        {
            //Fairly hacky code to load the ContentType as the real type instead of its base type, so it can be properly saved.
            if (Request.Path.ToLowerInvariant().Contains("editnodetypenew.aspx"))
            {
                _contentType = new DocumentType(docTypeId);
            }
            else if (Request.Path.ToLowerInvariant().Contains("editmediatype.aspx"))
            {
                _contentType = new cms.businesslogic.media.MediaType(docTypeId);
            }
            else if (Request.Path.ToLowerInvariant().Contains("editmembertype.aspx"))
            {
                _contentType = new cms.businesslogic.member.MemberType(docTypeId);
            }
            else
            {
                _contentType = new ContentType(docTypeId);
            }
        }

        /// <summary>
        /// Regenerates the XML caches. Used after a document type alias has been changed.
        /// </summary>
        /// <remarks>
        /// We only regenerate any XML cache based on if this is a Document type, not a media type or 
        /// a member type.
        /// </remarks>
        private void RegenerateXmlCaches()
        {
            _contentType.RebuildXmlStructuresForContent();

            //special case for DocumentType's
            if (_contentType is DocumentType)
            {
                library.RefreshContent();    
            }
        }

        private void UpdateTreeNode()
        {
            var clientTools = new ClientTools(this.Page);
            clientTools
                .SyncTree(_contentType.Path, true);
        }

        #region "Info" Pane


        private void SetupInfoPane()
        {
            InfoTabPage = TabView1.NewTabPage("Info");
            InfoTabPage.Controls.Add(pnlInfo);

            InfoTabPage.Style.Add("text-align", "center");

            ImageButton Save = InfoTabPage.Menu.NewImageButton();
            Save.Click += save_click;

            Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";
            Save.AlternateText = ui.Text("save", CurrentUser);
            Save.ID = "save";

            var dirInfo = new DirectoryInfo(UmbracoContext.Current.Server.MapPath(SystemDirectories.Umbraco + "/images/umbraco"));
            var fileInfo = dirInfo.GetFiles();

            var spriteFileNames = CMSNode.DefaultIconClasses.Select(IconClassToIconFileName).ToList();

            var diskFileNames = fileInfo.Select(FileNameToIconFileName).ToList();

            var listOfIcons = new List<ListItem>();

            // .sprNew was never intended to be in the document type editor
            foreach (var iconClass in CMSNode.DefaultIconClasses.Where(iconClass => iconClass.Equals(".sprNew", StringComparison.InvariantCultureIgnoreCase) == false))
            {
                // Still shows the selected even if we tell it to hide sprite duplicates so as not to break an existing selection
                if (_contentType.IconUrl.Equals(iconClass, StringComparison.InvariantCultureIgnoreCase) == false
                    && UmbracoSettings.IconPickerBehaviour == IconPickerBehaviour.HideSpriteDuplicates
                    && diskFileNames.Contains(IconClassToIconFileName(iconClass)))
                    continue;

                AddSpriteListItem(iconClass, listOfIcons);
            }

            foreach (var file in fileInfo)
            {
                // NH: don't show the sprite file
                if (file.Name.ToLowerInvariant() == "sprites.png".ToLowerInvariant() || file.Name.ToLowerInvariant() == "sprites_ie6.gif".ToLowerInvariant())
                    continue;

                // Still shows the selected even if we tell it to hide file duplicates so as not to break an existing selection
                if (_contentType.IconUrl.Equals(file.Name, StringComparison.InvariantCultureIgnoreCase) == false
                    && UmbracoSettings.IconPickerBehaviour == IconPickerBehaviour.HideFileDuplicates
                    && spriteFileNames.Contains(FileNameToIconFileName(file)))
                    continue;

                var listItemValue = ResolveClientUrl(SystemDirectories.Umbraco + "/images/umbraco/" + file.Name);

                AddFileListItem(file.Name, listItemValue, listOfIcons);
            }

            ddlIcons.Items.AddRange(listOfIcons.OrderBy(o => o.Text).ToArray());

            // Get thumbnails
            dirInfo = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Umbraco + "/images/thumbnails"));
            fileInfo = dirInfo.GetFiles();

            foreach (var file in fileInfo)
            {
                var li = new ListItem(file.Name);
                li.Attributes.Add("title", this.ResolveClientUrl(SystemDirectories.Umbraco + "/images/thumbnails/" + file.Name));

                if (this.Page.IsPostBack == false && li.Value == _contentType.Thumbnail)
                    li.Selected = true;

                ddlThumbnails.Items.Add(li);
            }

            Page.ClientScript.RegisterStartupScript(this.GetType(), "thumbnailsDropDown", string.Format(@"
function refreshDropDowns() {{
    jQuery('#{1}').msDropDown({{ showIcon: true, style: 'width:250px;' }});
    jQuery('#{3}').msDropDown({{ showIcon: false, rowHeight: '130', visibleRows: '2', style: 'width:250px;' }});
}}
jQuery(document).ready(function() {{ refreshDropDowns(); }});
", ddlIcons.ClientID, ddlIcons.ClientID, ddlIcons.ClientID, ddlThumbnails.ClientID, 500), true);
            txtName.Text = _contentType.GetRawText();
            txtAlias.Text = _contentType.Alias;
            description.Text = _contentType.GetRawDescription();

        }

        private void AddSpriteListItem(string iconClass, ICollection<ListItem> listOfIcons)
        {
            var li = new ListItem(
                      helper.SpaceCamelCasing((iconClass.Substring(1, iconClass.Length - 1)))
                      .Replace("Spr Tree", "")
                      .Trim(), iconClass);

            li.Attributes.Add("class", "spriteBackground sprTree " + iconClass.Trim('.'));
            li.Attributes.Add("style", "padding-left:24px !important; background-repeat:no-repeat; width:auto; height:auto;");

            AddListItem(listOfIcons, li);
        }

        private void AddFileListItem(string fileName, string listItemValue, ICollection<ListItem> listOfIcons)
        {
            var li = new ListItem(fileName, fileName);

            li.Attributes.Add("title", listItemValue);

            AddListItem(listOfIcons, li);
        }

        private void AddListItem(ICollection<ListItem> listOfIcons, ListItem li)
        {
            if (this.Page.IsPostBack == false && li.Value == _contentType.IconUrl)
                li.Selected = true;

            listOfIcons.Add(li);
        }

        private static string IconClassToIconFileName(string iconClass)
        {
            return iconClass.Substring(1, iconClass.Length - 1).ToLowerInvariant().Replace("sprTree".ToLowerInvariant(), "");
        }

        private static string FileNameToIconFileName(FileInfo file)
        {
            return file.Name.Substring(0, file.Name.LastIndexOf(".", StringComparison.Ordinal)).ToLowerInvariant();
        }
        #endregion


        #region "Structure" Pane
        private void SetupStructurePane()
        {
            dualAllowedContentTypes.ID = "allowedContentTypes";
            dualAllowedContentTypes.Width = 175;

            uicontrols.TabPage tp = TabView1.NewTabPage("Structure");
            tp.Controls.Add(pnlStructure);
            tp.Style.Add("text-align", "center");
            ImageButton Save = tp.Menu.NewImageButton();
            Save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
            Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";

            int[] allowedIds = _contentType.AllowedChildContentTypeIDs;
            if (!Page.IsPostBack)
            {
                string chosenContentTypeIDs = "";
                ContentType[] contentTypes = _contentType.GetAll();
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
            _contentType.AllowedChildContentTypeIDs = ids;
        }

        #endregion

        #region "Generic properties" Pane
        private void SetupGenericPropertiesPane()
        {
            GenericPropertiesTabPage = TabView1.NewTabPage("Generic properties");
            GenericPropertiesTabPage.Style.Add("text-align", "center");
            GenericPropertiesTabPage.Controls.Add(pnlProperties);

            ImageButton Save = GenericPropertiesTabPage.Menu.NewImageButton();
            Save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
            Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";

            //dlTabs.ItemCommand += new DataListCommandEventHandler(dlTabs_ItemCommand);
            BindDataGenericProperties(false);
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
        private void BindDataGenericProperties(bool Refresh)
        {
            cms.businesslogic.ContentType.TabI[] tabs = _contentType.getVirtualTabs;
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
                string tabCaption = t.ContentType == _contentType.Id ? t.GetRawCaption() : t.GetRawCaption() + " (inherited from " + new ContentType(t.ContentType).Text + ")";
                PropertyTypes.Controls.Add(new LiteralControl("<div class='genericPropertyListBox'><h2 class=\"propertypaneTitel\">Tab: " + tabCaption + "</h2>"));

                // zb-00036 #29889 : fix property types getter
                var propertyTypes = t.GetPropertyTypes(_contentType.Id, false);

                if (propertyTypes.Length > 0)
                {
                    HtmlInputHidden propSort = new HtmlInputHidden();
                    propSort.ID = "propSort_" + t.Id.ToString() + "_Content";
                    PropertyTypes.Controls.Add(propSort);
                    _sortLists.Add(propSort);

                    // zb-00036 #29889 : remove filter, not needed anymore

                    if (propertyTypes.Count() > 0)
                    {
                        PropertyTypes.Controls.Add(new LiteralControl("<ul class='genericPropertyList' id=\"t_" + t.Id.ToString() + "_Contents\">"));

                        foreach (cms.businesslogic.propertytype.PropertyType pt in propertyTypes)
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
                        AddNoPropertiesDefinedMessage();
                    }

                    PropertyTypes.Controls.Add(new LiteralControl("</div>"));
                }
                else
                {
                    AddNoPropertiesDefinedMessage();
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
            foreach (cms.businesslogic.propertytype.PropertyType pt in _contentType.PropertyTypes)
            {
                //This use to be:
                //if (pt.ContentTypeId == cType.Id && !inTab.ContainsKey(pt.Id.ToString())
                //But seriously, if it's not on a tab the tabId is 0, it's a lot easier to read IMO
                if (pt.ContentTypeId == _contentType.Id && pt.TabId == 0)
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

        private void AddNoPropertiesDefinedMessage()
        {
            PropertyTypes.Controls.Add(new LiteralControl("<div style=\"margin: 10px; padding: 4px; border: 1px solid #ccc;\">No properties defined on this tab. Click on the \"add a new property\" link at the top to create a new property.</div>"));
        }

        protected void gpw_Delete(object sender, System.EventArgs e)
        {
            GenericProperties.GenericPropertyWrapper gpw = (GenericProperties.GenericPropertyWrapper)sender;
            gpw.GenricPropertyControl.PropertyType.delete();
            _contentType = ContentType.GetContentType(_contentType.Id);
            this.BindDataGenericProperties(true);
        }
        
        private void SaveProperties(SaveClickEventArgs e)
        {
            this.CreateChildControls();

            GenericProperties.GenericProperty gpData = gp.GenricPropertyControl;
            if (gpData.Name.Trim() != "" && gpData.Alias.Trim() != "")
            {
                if (DoesPropertyTypeAliasExist(gpData))
                {
                    string[] info = { gpData.Name, gpData.Type.ToString() };
                    cms.businesslogic.propertytype.PropertyType pt = _contentType.AddPropertyType(cms.businesslogic.datatype.DataTypeDefinition.GetDataTypeDefinition(gpData.Type), Casing.SafeAliasWithForcingCheck(gpData.Alias.Trim()), gpData.Name);
                    pt.Description = gpData.Description;
                    pt.ValidationRegExp = gpData.Validation.Trim();
                    pt.Mandatory = gpData.Mandatory;

                    if (gpData.Tab != 0)
                    {
                        _contentType.SetTabOnPropertyType(pt, gpData.Tab);
                    }

                    gpData.Clear();

                }
                else
                {
                    e.Message = ui.Text("contentTypeDublicatePropertyType", CurrentUser);
                    e.IconType = umbraco.BasePages.BasePage.speechBubbleIcon.warning;
                }
            }

            foreach (GenericProperties.GenericPropertyWrapper gpw in _genericProperties)
            {
                cms.businesslogic.propertytype.PropertyType pt = gpw.PropertyType;
                pt.Alias = gpw.GenricPropertyControl.Alias;
                pt.Name = gpw.GenricPropertyControl.Name;
                pt.Description = gpw.GenricPropertyControl.Description;
                pt.ValidationRegExp = gpw.GenricPropertyControl.Validation.Trim();
                pt.Mandatory = gpw.GenricPropertyControl.Mandatory;
                pt.DataTypeDefinition = cms.businesslogic.datatype.DataTypeDefinition.GetDataTypeDefinition(gpw.GenricPropertyControl.Type);
                if (gpw.GenricPropertyControl.Tab == 0)
                    _contentType.removePropertyTypeFromTab(pt);
                else
                    _contentType.SetTabOnPropertyType(pt, gpw.GenricPropertyControl.Tab);

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

        private bool DoesPropertyTypeAliasExist(GenericProperty gpData)
        {
            bool hasAlias = _contentType.getPropertyType(Casing.SafeAliasWithForcingCheck(gpData.Alias.Trim())) != null;
            ContentType ct = _contentType;
            while (ct.MasterContentType > 0)
            {
                ct = new ContentType(ct.MasterContentType);
                hasAlias = ct.getPropertyType(Casing.SafeAliasWithForcingCheck(gpData.Alias.Trim())) != null;
            }
            return !hasAlias;
        }

        public bool HasRows(System.Data.DataView dv)
        {
            return (dv.Count == 0);
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
                BindDataGenericProperties(false);
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
        private void SetupTabPane()
        {
            uicontrols.TabPage tp = TabView1.NewTabPage("Tabs");


            pnlTab.Style.Add("text-align", "center");
            tp.Controls.Add(pnlTab);
            ImageButton Save = tp.Menu.NewImageButton();
            Save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
            Save.ID = "SaveButton";
            Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";
            BindTabs();
        }

        private void BindTabs()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("name");
            dt.Columns.Add("id");
            dt.Columns.Add("order");
            foreach (cms.businesslogic.ContentType.TabI tb in _contentType.getVirtualTabs.ToList())
            {
                if (tb.ContentType == _contentType.Id)
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
                    BindTabs();

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

        private DataTable _dataTypeTable;

        public DataTable DataTypeTable
        {
            get
            {
                if (_dataTypeTable == null)
                {
                    _dataTypeTable = new DataTable();
                    _dataTypeTable.Columns.Add("name");
                    _dataTypeTable.Columns.Add("id");
                    foreach (cms.businesslogic.datatype.DataTypeDefinition DataType in cms.businesslogic.datatype.DataTypeDefinition.GetAll())
                    {
                        DataRow dr = _dataTypeTable.NewRow();
                        dr["name"] = DataType.Text;
                        dr["id"] = DataType.Id.ToString();
                        _dataTypeTable.Rows.Add(dr);
                    }
                }
                return _dataTypeTable;
            }
        }


        protected void btnNewTab_Click(object sender, System.EventArgs e)
        {
            if (txtNewTab.Text.Trim() != "")
            {
                _contentType.AddVirtualTab(txtNewTab.Text);
                _contentType = new ContentType(_contentType.Id);
                SaveClickEventArgs ea = new SaveClickEventArgs(ui.Text("contentTypeTabCreated", CurrentUser));
                ea.IconType = umbraco.BasePages.BasePage.speechBubbleIcon.success;

                RaiseBubbleEvent(new object(), ea);

                txtNewTab.Text = "";
                BindTabs();
                BindDataGenericProperties(true);
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
                _contentType.DeleteVirtualTab(int.Parse(e.Item.Cells[0].Text));

                SaveClickEventArgs ea = new SaveClickEventArgs(ui.Text("contentTypeTabDeleted", CurrentUser));
                ea.IconType = umbraco.BasePages.BasePage.speechBubbleIcon.success;

                RaiseBubbleEvent(new object(), ea);

            }


            BindTabs();
            BindDataGenericProperties(true);
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
                _contentType.SetTabName(tabid, tabName);
                if (Int32.TryParse(((TextBox)dgi.FindControl("txtSortOrder")).Text, out tabSortOrder))
                {
                    _contentType.SetTabSortOrder(tabid, tabSortOrder);
                }
            }
        }

        #endregion

        /// <summary>
        /// TabView1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.TabView TabView1;

        /// <summary>
        /// pnlGeneral control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel pnlGeneral;

        /// <summary>
        /// pnlTab control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel pnlTab;

        /// <summary>
        /// PaneTabsInherited control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane PaneTabsInherited;

        /// <summary>
        /// tabsMasterContentTypeName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal tabsMasterContentTypeName;

        /// <summary>
        /// Pane2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane Pane2;

        /// <summary>
        /// pp_newTab control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_newTab;

        /// <summary>
        /// txtNewTab control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox txtNewTab;

        /// <summary>
        /// btnNewTab control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button btnNewTab;

        /// <summary>
        /// Pane1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane Pane1;

        /// <summary>
        /// dgTabs control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DataGrid dgTabs;

        /// <summary>
        /// lttNoTabs control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal lttNoTabs;

        /// <summary>
        /// pnlInfo control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel pnlInfo;

        /// <summary>
        /// Pane3 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane Pane3;

        /// <summary>
        /// pp_name control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_name;

        /// <summary>
        /// txtName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox txtName;

        /// <summary>
        /// RequiredFieldValidator1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RequiredFieldValidator RequiredFieldValidator1;

        /// <summary>
        /// pp_alias control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_alias;

        /// <summary>
        /// txtAlias control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox txtAlias;

        /// <summary>
        /// pp_icon control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_icon;

        /// <summary>
        /// ddlIcons control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DropDownList ddlIcons;

        /// <summary>
        /// pp_thumbnail control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_thumbnail;

        /// <summary>
        /// ddlThumbnails control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DropDownList ddlThumbnails;

        /// <summary>
        /// pp_description control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_description;

        /// <summary>
        /// description control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox description;

        /// <summary>
        /// pnlStructure control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel pnlStructure;

        /// <summary>
        /// Pane5 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane Pane5;

        /// <summary>
        /// pp_allowedChildren control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_allowedChildren;

        /// <summary>
        /// lstAllowedContentTypes control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBoxList lstAllowedContentTypes;

        /// <summary>
        /// PlaceHolderAllowedContentTypes control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder PlaceHolderAllowedContentTypes;

        /// <summary>
        /// pnlProperties control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel pnlProperties;

        /// <summary>
        /// PanePropertiesInherited control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane PanePropertiesInherited;

        /// <summary>
        /// propertiesMasterContentTypeName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal propertiesMasterContentTypeName;

        /// <summary>
        /// Pane4 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane Pane4;

        /// <summary>
        /// PropertyTypeNew control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder PropertyTypeNew;

        /// <summary>
        /// PropertyTypes control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder PropertyTypes;

        /// <summary>
        /// theClientId control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal theClientId;

    }
}