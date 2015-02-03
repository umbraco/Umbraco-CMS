using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Strings;
using Umbraco.Web.UI.Controls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.cms.helpers;
using umbraco.controls.GenericProperties;
using Umbraco.Core.IO;
using umbraco.presentation;
using umbraco.BasePages;
using Constants = Umbraco.Core.Constants;
using ContentType = umbraco.cms.businesslogic.ContentType;
using PropertyType = Umbraco.Core.Models.PropertyType;

namespace umbraco.controls
{

    [ClientDependency(ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "ui/jquery.dd.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "ui/dd.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "GenericProperty/genericproperty.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "GenericProperty/genericproperty.js", "UmbracoClient")]
    public partial class ContentTypeControlNew : UmbracoUserControl
    {
        // General Private members
        private ContentType _contentType;
        private static string UmbracoPath = SystemDirectories.Umbraco;
        public bool HideStructure { get; set; }
        public Func<DocumentType, DocumentType> DocumentTypeCallback { get; set; }

        protected string ContentTypeAlias
        {
            get { return _contentType.Alias; }
        }
        protected int ContentTypeId
        {
            get { return _contentType.Id; }
        }

        // "Tab" tab
        protected uicontrols.Pane Pane8;

        // "Structure" tab
        protected DualSelectbox DualAllowedContentTypes = new DualSelectbox();

        // "Structure" tab - Compositions
        protected DualSelectbox DualContentTypeCompositions = new DualSelectbox();

        // "Info" tab
        public uicontrols.TabPage InfoTabPage;

        // "Generic properties" tab
        public uicontrols.TabPage GenericPropertiesTabPage;

        public GenericPropertyWrapper gp;
        private DataTable _dataTypeTable;
        private ArrayList _genericProperties = new ArrayList();
        private ArrayList _sortLists = new ArrayList();

        //the async saving task
        private Action<SaveAsyncState> _asyncSaveTask;
        //the async delete property task
        private Action<DeleteAsyncState> _asyncDeleteTask;

        internal event SavingContentTypeEventHandler SavingContentType;
        internal delegate void SavingContentTypeEventHandler(ContentType e);

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            LoadContentType();

            SetupInfoPane();

            if (HideStructure)
            {
                pnlStructure.Visible = false;
            }
            else
            {
                SetupStructurePane();
                SetupCompositionsPane();
            }

            SetupGenericPropertiesPane();
            SetupTabPane();

            // [ClientDependency(ClientDependencyType.Javascript, "js/UmbracoCasingRules.aspx", "UmbracoRoot")]
            var loader = ClientDependency.Core.Controls.ClientDependencyLoader.GetInstance(new HttpContextWrapper(Context));
            var helper = new UrlHelper(new RequestContext(new HttpContextWrapper(Context), new RouteData()));
            loader.RegisterDependency(helper.GetCoreStringsControllerPath() + "ServicesJavaScript", ClientDependencyType.Javascript);
        }        

        protected void Page_Load(object sender, EventArgs e)
        {
            pp_newTab.Text = ui.Text("newtab", Security.CurrentUser);
            pp_alias.Text = ui.Text("alias", Security.CurrentUser);
            pp_name.Text = ui.Text("name", Security.CurrentUser);
            pp_allowedChildren.Text = ui.Text("allowedchildnodetypes", Security.CurrentUser);
            pp_compositions.Text = ui.Text("contenttypecompositions", Security.CurrentUser);
            pp_description.Text = ui.Text("editcontenttype", "description", Security.CurrentUser);
            pp_icon.Text = ui.Text("icon", Security.CurrentUser);
            pp_Root.Text = ui.Text("editcontenttype", "allowAtRoot", Security.CurrentUser) + "<br/><small>" + ui.Text("editcontenttype", "allowAtRootDesc", Security.CurrentUser) + "</small>";
            pp_isContainer.Text = ui.Text("editcontenttype", "hasListView", Security.CurrentUser) + "<br/><small>" + ui.Text("editcontenttype", "hasListViewDesc", Security.CurrentUser) + "</small>";

            // we'll disable this...
            if (!Page.IsPostBack && _contentType.MasterContentType != 0)
            {
                string masterName = ContentType.GetContentType(_contentType.MasterContentType).Text;
                tabsMasterContentTypeName.Text = masterName;
                propertiesMasterContentTypeName.Text = masterName;
                PaneTabsInherited.Visible = true;
                PanePropertiesInherited.Visible = true;
            }

            if (string.IsNullOrEmpty(_contentType.IconUrl))
                lt_icon.Text = "icon-document";
            else
                lt_icon.Text = _contentType.IconUrl.TrimStart('.');

            checkTxtAliasJs.Text = string.Format("checkAlias('#{0}');", txtAlias.ClientID);

        }
        
        /// <summary>
        /// A class to track the async state for deleting a doc type property
        /// </summary>
        private class DeleteAsyncState
        {
            public Umbraco.Web.UmbracoContext UmbracoContext { get; private set; }
            public GenericPropertyWrapper GenericPropertyWrapper { get; private set; }

            public DeleteAsyncState(
                Umbraco.Web.UmbracoContext umbracoContext,
                GenericPropertyWrapper genericPropertyWrapper)
            {
                UmbracoContext = umbracoContext;
                GenericPropertyWrapper = genericPropertyWrapper;
            }
        }

        /// <summary>
        /// A class to track the async state for saving the doc type
        /// </summary>
        private class SaveAsyncState
        {
            public SaveAsyncState(
                Umbraco.Web.UmbracoContext umbracoContext,
                SaveClickEventArgs saveArgs, 
                string originalAlias, 
                string originalName,
                string newAlias,
                string newName,
                string[] originalPropertyAliases)
            {
                UmbracoContext = umbracoContext;
                SaveArgs = saveArgs;
                _originalAlias = originalAlias;
                _originalName = originalName;
                _newAlias = newAlias;
                _originalPropertyAliases = originalPropertyAliases;
                _newName = newName;
            }

            public Umbraco.Web.UmbracoContext UmbracoContext { get; private set; }
            public SaveClickEventArgs SaveArgs { get; private set; }
            private readonly string _originalAlias;
            private readonly string _originalName;
            private readonly string _newAlias;
            private readonly string _newName;
            private readonly string[] _originalPropertyAliases;


            public bool HasAliasChanged()
            {
                return (string.Compare(_originalAlias, _newAlias, StringComparison.OrdinalIgnoreCase) != 0);
            }
            public bool HasNameChanged()
            {
                return (string.Compare(_originalName, _newName, StringComparison.OrdinalIgnoreCase) != 0);
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
            BindTabs();
            BindDataGenericProperties(true);

            // we need to re-bind the alias as the SafeAlias method can have changed it
            txtAlias.Text = _contentType.Alias;

            //Notify the parent control
            RaiseBubbleEvent(new object(), state.SaveArgs);

            if (state.HasNameChanged())
                UpdateTreeNode();

            Trace.Write("ContentTypeControlNew", "async operation ended");

            //complete it
            _asyncSaveTask.EndInvoke(ar);
        }

        /// <summary>
        /// The save button click event handlers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void save_click(object sender, EventArgs e)
        {

            //sync state betweet lt and hidden value
            lt_icon.Text = tb_icon.Value;

            var state = new SaveAsyncState(
                UmbracoContext,
                new SaveClickEventArgs("Saved")
                {
                    IconType = BasePage.speechBubbleIcon.success
                }, _contentType.Alias, _contentType.Text, txtAlias.Text, txtName.Text, _contentType.PropertyTypes.Select(x => x.Alias).ToArray());

            var isMediaType = Request.Path.ToLowerInvariant().Contains("editmediatype.aspx");

            //Add the async operation to the page
            //NOTE: Must pass in a null and do not pass in a true to the 'executeInParallel', this is changed in .net 4.5 for the better, otherwise you'll get a ysod.
            Page.RegisterAsyncTask(new PageAsyncTask(BeginAsyncSaveOperation, EndAsyncSaveOperation, null, state));
            
            //create the save task to be executed async
            _asyncSaveTask = asyncState =>
                {
                    Trace.Write("ContentTypeControlNew", "executing task");

                    //we need to re-set the UmbracoContext since it will be nulled and our cache handlers need it
                    global::Umbraco.Web.UmbracoContext.Current = asyncState.UmbracoContext;

                    _contentType.ContentTypeItem.Name = txtName.Text;
                    _contentType.ContentTypeItem.Alias = txtAlias.Text; // raw, contentType.Alias takes care of it
                        _contentType.ContentTypeItem.Icon = tb_icon.Value;
                    _contentType.ContentTypeItem.Description = description.Text;
                    //_contentType.ContentTypeItem.Thumbnail = ddlThumbnails.SelectedValue;
                    _contentType.ContentTypeItem.AllowedAsRoot = allowAtRoot.Checked;
                        _contentType.ContentTypeItem.IsContainer = cb_isContainer.Checked;

                    int i = 0;
                    var ids = SaveAllowedChildTypes();
                    _contentType.ContentTypeItem.AllowedContentTypes = ids.Select(x => new ContentTypeSort {Id = new Lazy<int>(() => x), SortOrder = i++});

                    // figure out whether compositions are locked
                    var allContentTypes = Request.Path.ToLowerInvariant().Contains("editmediatype.aspx")
                        ? ApplicationContext.Services.ContentTypeService.GetAllMediaTypes().Cast<IContentTypeComposition>().ToArray()
                        : ApplicationContext.Services.ContentTypeService.GetAllContentTypes().Cast<IContentTypeComposition>().ToArray();
                    var isUsing = allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == _contentType.Id)).ToArray();

                    // if compositions are locked, do nothing (leave them as they are)
                    // else process the checkbox list and add/remove compositions accordingly

                    if (isUsing.Length == 0)
                    {
                        //Saving ContentType Compositions
                        var compositionIds = SaveCompositionContentTypes();
                        var existingCompsitionIds = _contentType.ContentTypeItem.CompositionIds().ToList();
                        if (compositionIds.Any())
                        {
                            // if some compositions were checked in the list, iterate over them
                            foreach (var compositionId in compositionIds)
                            {
                                // ignore if it is the current content type
                                if (_contentType.Id.Equals(compositionId)) continue;

                                // ignore if it is already a composition of the content type
                                if (existingCompsitionIds.Any(x => x.Equals(compositionId))) continue;

                                // add to the content type compositions
                                var compositionType = isMediaType
                                    ? Services.ContentTypeService.GetMediaType(compositionId).SafeCast<IContentTypeComposition>()
                                    : Services.ContentTypeService.GetContentType(compositionId).SafeCast<IContentTypeComposition>();
                                try
                                {
                                    //TODO if added=false then return error message
                                    var added = _contentType.ContentTypeItem.AddContentType(compositionType);
                                }
                                catch (InvalidCompositionException ex)
                                {
                                    state.SaveArgs.IconType = BasePage.speechBubbleIcon.error;
                                    state.SaveArgs.Message = ex.Message;
                                }
                            }

                            // then iterate over removed = existing except checked
                            var removeIds = existingCompsitionIds.Except(compositionIds);
                            foreach (var removeId in removeIds)
                            {
                                // and remove from the content type composition
                                var compositionType = isMediaType
                                    ? Services.ContentTypeService.GetMediaType(removeId).SafeCast<IContentTypeComposition>()
                                    : Services.ContentTypeService.GetContentType(removeId).SafeCast<IContentTypeComposition>();
                                var removed = _contentType.ContentTypeItem.RemoveContentType(compositionType.Alias);
                            }
                        }
                        else if (existingCompsitionIds.Any())
                        {
                            // else none were checked - if the content type had compositions,
                            // iterate over them all and remove them
                            var removeIds = existingCompsitionIds.Except(compositionIds); // except here makes no sense?
                            foreach (var removeId in removeIds)
                            {
                                // remove existing
                                var compositionType = isMediaType
                                    ? Services.ContentTypeService.GetMediaType(removeId).SafeCast<IContentTypeComposition>()
                                    : Services.ContentTypeService.GetContentType(removeId).SafeCast<IContentTypeComposition>();
                                var removed = _contentType.ContentTypeItem.RemoveContentType(compositionType.Alias);
                            }
                        }
                    }

                    var tabs = SaveTabs(); // returns { TabId, TabName, TabSortOrder }
                    foreach (var tab in tabs)
                    {
                        var group = _contentType.ContentTypeItem.PropertyGroups.FirstOrDefault(x => x.Id == tab.Item1);
                        if (group == null)
                        {
                            // creating a group
                            group = new PropertyGroup {Id = tab.Item1, Name = tab.Item2, SortOrder = tab.Item3};
                            _contentType.ContentTypeItem.PropertyGroups.Add(group);
                        }
                        else
                        {
                            // updating an existing group
                            group.Name = tab.Item2;
                            group.SortOrder = tab.Item3;
                        }
                    }

                    SavePropertyType(asyncState.SaveArgs, _contentType.ContentTypeItem);
                    //SavePropertyType(state.SaveArgs, _contentType.ContentTypeItem);
                    UpdatePropertyTypes(_contentType.ContentTypeItem);

                    if (DocumentTypeCallback != null)
                    {
                        var documentType = _contentType as DocumentType;
                        if (documentType != null)
                        {
                            var result = DocumentTypeCallback(documentType);
                        }
                    }

                    if (SavingContentType != null)
                    {
                        SavingContentType(_contentType);
                    }

                    try
                    {
                        _contentType.Save();
                    }
                    catch (DuplicateNameException ex)
                    {
                        DuplicateAliasValidator.IsValid = false;
                        //asyncState.SaveArgs.IconType = BasePage.speechBubbleIcon.error;
                        state.SaveArgs.IconType = BasePage.speechBubbleIcon.error;
                        //asyncState.SaveArgs.Message = ex.Message;
                        state.SaveArgs.Message = ex.Message;
                        return;
                    }
                    catch (Exception ex)
                    {
                        state.SaveArgs.IconType = BasePage.speechBubbleIcon.error;
                        state.SaveArgs.Message = ex.Message;
                    }

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
        /// Updates the Node in the Tree
        /// </summary>
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

            var Save = TabView1.Menu.NewButton();
            Save.Click += save_click;
            Save.Text = ui.Text("save", Security.CurrentUser);
            Save.ID = "save";
            Save.ButtonType = uicontrols.MenuButtonType.Primary;

            txtName.Text = _contentType.GetRawText();
            txtAlias.Text = _contentType.Alias;
            description.Text = _contentType.GetRawDescription();
            tb_icon.Value = _contentType.IconUrl;
            
            if(string.IsNullOrEmpty(_contentType.IconUrl))
                lt_icon.Text = "icon-document";
            else
                lt_icon.Text = _contentType.IconUrl.TrimStart('.');

            /*
            var dirInfo = new DirectoryInfo(Server.MapPath(SystemDirectories.Umbraco + "/images/umbraco"));
            var fileInfo = dirInfo.GetFiles();

            var spriteFileNames = CMSNode.DefaultIconClasses.Select(IconClassToIconFileName).ToList();
            var diskFileNames = fileInfo.Select(FileNameToIconFileName).ToList();
            var listOfIcons = new List<ListItem>();
            // .sprNew was never intended to be in the document type editor
            foreach (var iconClass in CMSNode.DefaultIconClasses.Where(iconClass => iconClass.Equals(".sprNew", StringComparison.InvariantCultureIgnoreCase) == false))
            {
                // Still shows the selected even if we tell it to hide sprite duplicates so as not to break an existing selection
                if (_contentType.IconUrl.Equals(iconClass, StringComparison.InvariantCultureIgnoreCase) == false
                    && UmbracoConfiguration.Current.UmbracoSettings.Content.IconPickerBehaviour == IconPickerBehaviour.HideSpriteDuplicates
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
                    && UmbracoConfiguration.Current.UmbracoSettings.Content.IconPickerBehaviour == IconPickerBehaviour.HideFileDuplicates
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

               // ddlThumbnails.Items.Add(li);
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
            */
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
            DualAllowedContentTypes.ID = "allowedContentTypes";
            DualAllowedContentTypes.Width = 175;

            uicontrols.TabPage tp = TabView1.NewTabPage("Structure");
            tp.Controls.Add(pnlStructure);
            
            int[] allowedIds = _contentType.AllowedChildContentTypeIDs;
            if (!Page.IsPostBack)
            {
                string chosenContentTypeIDs = "";
                ContentType[] contentTypes = _contentType.GetAll();
                foreach (ContentType ct in contentTypes.OrderBy(x => x.Text))
                {
                    ListItem li = new ListItem(ct.Text, ct.Id.ToString());
                    DualAllowedContentTypes.Items.Add(li);
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
                DualAllowedContentTypes.Value = chosenContentTypeIDs;
            }

            allowAtRoot.Checked = _contentType.AllowAtRoot;
            cb_isContainer.Checked = _contentType.IsContainerContentType;
        }

        private int[] SaveAllowedChildTypes()
        {
            var tmp = new ArrayList();
            foreach (ListItem li in lstAllowedContentTypes.Items)
            {
                if (li.Selected)
                    tmp.Add(int.Parse(li.Value));
            }
            var ids = new int[tmp.Count];
            for (int i = 0; i < tmp.Count; i++) ids[i] = (int)tmp[i];

            return ids;
        }

        #endregion

        #region Compositions Pane

        // returns content type compositions, recursively
        // return each content type once and only once
        private IEnumerable<IContentTypeComposition> GetIndirect(IContentTypeComposition ctype)
        {
            // hashset guarantees unicity on Id
            var all = new HashSet<IContentTypeComposition>(new DelegateEqualityComparer<IContentTypeComposition>(
                (x, y) => x.Id == y.Id,
                x => x.Id));

            var stack = new Stack<IContentTypeComposition>();

            foreach (var x in ctype.ContentTypeComposition)
                stack.Push(x);

            while (stack.Count > 0)
            {
                var x = stack.Pop();
                all.Add(x);
                foreach (var y in x.ContentTypeComposition)
                    stack.Push(y);
            }

            return all;
        }

        private void SetupCompositionsPane()
        {
            DualContentTypeCompositions.ID = "compositionContentTypes";
            DualContentTypeCompositions.Width = 175;

            // fix for 7.2 - only top-level content types can be used as mixins

            if (Page.IsPostBack == false)
            {
                var allContentTypes = Request.Path.ToLowerInvariant().Contains("editmediatype.aspx")
                    ? ApplicationContext.Services.ContentTypeService.GetAllMediaTypes().Cast<IContentTypeComposition>().ToArray()
                    : ApplicationContext.Services.ContentTypeService.GetAllContentTypes().Cast<IContentTypeComposition>().ToArray();

                // note: there are many sanity checks missing here and there ;-((
                // make sure once and for all
                //if (allContentTypes.Any(x => x.ParentId > 0 && x.ContentTypeComposition.Any(y => y.Id == x.ParentId) == false))
                //    throw new Exception("A parent does not belong to a composition.");

                // find out if any content type uses this content type
                var isUsing = allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == _contentType.Id)).ToArray();
                if (isUsing.Length > 0)
                {
                    // if it is used then it has to remain top-level

                    // no composition is possible at all
                    DualContentTypeCompositions.Items.Clear();
                    lstContentTypeCompositions.Items.Clear();
                    DualContentTypeCompositions.Value = "";

                    PlaceHolderContentTypeCompositions.Controls.Add(new Literal { Text = "<em>This content type is used as a parent and/or in "
                        + "a composition, and therefore cannot be composed itself.<br /><br />Used by: " 
                        + string.Join(", ", isUsing.Select(x => x.Name))
                        + "</em>" });
                }
                else
                {
                    // if it is not used then composition is possible

                    // hashset guarantees unicity on Id
                    var list = new HashSet<IContentTypeComposition>(new DelegateEqualityComparer<IContentTypeComposition>(
                        (x, y) => x.Id == y.Id,
                        x => x.Id));

                    // usable types are those that are top-level
                    var usableContentTypes = allContentTypes
                        .Where(x => x.ContentTypeComposition.Any() == false).ToArray();
                    foreach (var x in usableContentTypes)
                        list.Add(x);

                    // indirect types are those that we use, directly or indirectly
                    var indirectContentTypes = GetIndirect(_contentType.ContentTypeItem).ToArray();
                    foreach (var x in indirectContentTypes)
                        list.Add(x);

                    // directContentTypes are those we use directly
                    // they are already in indirectContentTypes, no need to add to the list
                    var directContentTypes = _contentType.ContentTypeItem.ContentTypeComposition.ToArray();

                    var enabled = usableContentTypes.Select(x => x.Id) // those we can use
                        .Except(indirectContentTypes.Select(x => x.Id)) // except those that are indirectly used
                        .Union(directContentTypes.Select(x => x.Id)) // but those that are directly used
                        .Where(x => x != _contentType.ParentId) // but not the parent
                        .Distinct()
                        .ToArray();

                    var wtf = new List<int>();
                    foreach (var contentType in list.OrderBy(x => x.Name).Where(x => x.Id != _contentType.Id))
                    {
                        var li = new ListItem(contentType.Name, contentType.Id.ToInvariantString())
                        {
                            // disable parent and anything that's not usable
                            Enabled = enabled.Contains(contentType.Id),
                            // select
                            Selected = indirectContentTypes.Any(x => x.Id == contentType.Id)
                        };

                        DualContentTypeCompositions.Items.Add(li);
                        lstContentTypeCompositions.Items.Add(li);

                        if (li.Selected)
                            wtf.Add(contentType.Id);
                    }
                    DualContentTypeCompositions.Value = string.Join(",", wtf);
                }
            }

            //int[] compositionIds = _contentType.ContentTypeItem.CompositionIds().ToArray();
            //if (!Page.IsPostBack)
            //{
            //    string chosenContentTypeIDs = "";
            //    ContentType[] contentTypes = _contentType.GetAll();
            //    foreach (ContentType ct in contentTypes.OrderBy(x => x.Text))
            //    {
            //        ListItem li = new ListItem(ct.Text, ct.Id.ToString());
            //        if (ct.Id == _contentType.Id)
            //            li.Enabled = false;

            //        DualContentTypeCompositions.Items.Add(li);
            //        lstContentTypeCompositions.Items.Add(li);
                    
            //        foreach (int i in compositionIds)
            //        {
            //            if (i == ct.Id)
            //            {
            //                li.Selected = true;
            //                chosenContentTypeIDs += ct.Id + ",";
            //            }
            //        }
            //    }
            //    DualContentTypeCompositions.Value = chosenContentTypeIDs;
            //}
        }

        private int[] SaveCompositionContentTypes()
        {
            return lstContentTypeCompositions.Items.Cast<ListItem>()
                .Where(x => x.Selected)
                .Select(x => int.Parse(x.Value))
                .ToArray();
        }

        #endregion

        #region "Generic properties" Pane

        private void SetupGenericPropertiesPane()
        {
            GenericPropertiesTabPage = TabView1.NewTabPage("Generic properties");
            GenericPropertiesTabPage.Controls.Add(pnlProperties);
            BindDataGenericProperties(false);
        }

        private void BindDataGenericProperties(bool refresh)
        {
            var tabs = _contentType.getVirtualTabs;
            var propertyTypeGroups = _contentType.PropertyTypeGroups.ToList();
            var dtds = cms.businesslogic.datatype.DataTypeDefinition.GetAll();

            PropertyTypes.Controls.Clear();

            // Add new property
            if (PropertyTypeNew.Controls.Count == 0)
            {
                PropertyTypeNew.Controls.Add(new LiteralControl("<h2 class=\"propertypaneTitel\">Add New Property</h2><ul class='genericPropertyList addNewProperty'>"));
                gp = new GenericPropertyWrapper();
                gp.ID = "GenericPropertyNew";
                gp.Tabs = tabs;
                gp.DataTypeDefinitions = dtds;
                PropertyTypeNew.Controls.Add(gp);
                PropertyTypeNew.Controls.Add(new LiteralControl("</ul>"));
            }
            else if (refresh)
            {
                gp = (GenericPropertyWrapper)PropertyTypeNew.Controls[1];
                gp.ID = "GenericPropertyNew";
                gp.Tabs = tabs;
                gp.DataTypeDefinitions = dtds;
                gp.UpdateEditControl();
                gp.GenricPropertyControl.UpdateInterface();
                gp.GenricPropertyControl.Clear();
            }

            _genericProperties.Clear();
            var inTab = new Hashtable();
            int counter = 0;

            PropertyTypes.Controls.Add(new LiteralControl("<div id='tabs-container'>"));  // opens draggable container for properties on tabs

            foreach (var tab in tabs)
            {
                string tabName = tab.GetRawCaption();
                string tabCaption = tabName;
                if (tab.ContentType != _contentType.Id) 
                {
                    tabCaption += " (inherited from " + new ContentType(tab.ContentType).Text + ")";
                }

                PropertyTypes.Controls.Add(new LiteralControl("<div class='genericPropertyListBox'><h2 data-tabname='" + tabName + "' class=\"propertypaneTitel\">Tab: " + tabCaption + "</h2>"));

                var propertyGroup = propertyTypeGroups.SingleOrDefault(x => x.ParentId == tab.Id);
                var propertyTypes = (propertyGroup == null
                    ? tab.GetPropertyTypes(_contentType.Id, false)
                    : propertyGroup.GetPropertyTypes()).ToArray();

                var propertyGroupId = tab.Id;

                var propSort = new HtmlInputHidden();
                propSort.ID = "propSort_" + propertyGroupId + "_Content";
                PropertyTypes.Controls.Add(propSort);
                _sortLists.Add(propSort);

                if (propertyTypes.Any(x => x.ContentTypeId == _contentType.Id))
                {
                    PropertyTypes.Controls.Add(new LiteralControl("<ul class='genericPropertyList' id=\"t_" + propertyGroupId.ToString() + "_Contents\">"));

                    foreach (var pt in propertyTypes)
                    {
                        //If the PropertyType doesn't belong on this ContentType skip it and continue to the next one
                        if (pt.ContentTypeId != _contentType.Id) continue;

                        cms.businesslogic.datatype.DataTypeDefinition[] filteredDtds;
                        var gpw = GetPropertyWrapperForPropertyType(pt, dtds, out filteredDtds);
                        gpw.ID = "gpw_" + pt.Id;
                        gpw.PropertyType = pt;
                        gpw.Tabs = tabs;
                        gpw.TabId = propertyGroupId;
                        gpw.DataTypeDefinitions = filteredDtds;
                        gpw.Delete += gpw_Delete;
                        gpw.FullId = "t_" + propertyGroupId + "_Contents_" + +pt.Id;

                        PropertyTypes.Controls.Add(gpw);
                        _genericProperties.Add(gpw);
                        if (refresh)
                            gpw.GenricPropertyControl.UpdateInterface();

                        inTab.Add(pt.Id.ToString(CultureInfo.InvariantCulture), "");
                        counter++;
                    }

                    PropertyTypes.Controls.Add(new LiteralControl("</ul>"));
                }
                else
                {
                    AddNoPropertiesDefinedMessage();
                }

                var jsSortable = GetJavaScriptForPropertySorting(propSort.ClientID);
                Page.ClientScript.RegisterStartupScript(this.GetType(), propSort.ClientID, jsSortable, true);

                PropertyTypes.Controls.Add(new LiteralControl("</div>"));
            }

            // Generic properties tab
            counter = 0;
            bool propertyTabHasProperties = false;
            var propertiesPh = new PlaceHolder();
            propertiesPh.ID = "propertiesPH";
            PropertyTypes.Controls.Add(new LiteralControl("<h2 data-tabname=\"Generic Properties\" class=\"propertypaneTitel\">Tab: Generic Properties</h2>"));
            PropertyTypes.Controls.Add(propertiesPh);

            var propSortGp = new HtmlInputHidden();
            propSortGp.ID = "propSort_general_Content";
            PropertyTypes.Controls.Add(propSortGp);
            _sortLists.Add(propSortGp);
            
            propertiesPh.Controls.Add(new LiteralControl("<ul class='genericPropertyList' id=\"t_general_Contents\">"));
            foreach (var pt in _contentType.PropertyTypes)
            {
                //This use to be:
                if (pt.ContentTypeId == _contentType.Id && inTab.ContainsKey(pt.Id.ToString(CultureInfo.InvariantCulture)) == false)
                //But seriously, if it's not on a tab the tabId is 0, it's a lot easier to read IMO
                //if (pt.ContentTypeId == _contentType.Id && pt.TabId == 0)
                {
                    cms.businesslogic.datatype.DataTypeDefinition[] filteredDtds;
                    var gpw = GetPropertyWrapperForPropertyType(pt, dtds, out filteredDtds);

                    // Changed by duckie, was:
                    // gpw.ID = "gpw_" + editPropertyType.Alias;
                    // Which is NOT unique!
                    gpw.ID = "gpw_" + pt.Id;

                    gpw.PropertyType = pt;
                    gpw.Tabs = tabs;
                    gpw.DataTypeDefinitions = filteredDtds;
                    gpw.Delete += new EventHandler(gpw_Delete);
                    gpw.FullId = "t_general_Contents_" + pt.Id;

                    propertiesPh.Controls.Add(gpw);
                    _genericProperties.Add(gpw);
                    if (refresh)
                        gpw.GenricPropertyControl.UpdateInterface();
                    inTab.Add(pt.Id, "");
                    propertyTabHasProperties = true;
                    counter++;
                }
            }

            propertiesPh.Controls.Add(new LiteralControl("</ul>"));

            var jsSortable_gp = GetJavaScriptForPropertySorting(propSortGp.ClientID);

            Page.ClientScript.RegisterStartupScript(this.GetType(), "propSort_gp", jsSortable_gp, true);


            if (!propertyTabHasProperties)
            {
                AddNoPropertiesDefinedMessage();
                PropertyTypes.Controls.Remove(PropertyTypes.FindControl("propertiesPH"));
            }
            else
            {
                PropertyTypes.Controls.Add(propertiesPh);
            }

            PropertyTypes.Controls.Add(new LiteralControl("</div>")); // closes draggable container for properties on tabs

        }

        /// <summary>
        /// Returns a generic property wrapper for a given property - this determines if the property type should be
        /// allowed to be editable.
        /// </summary>
        /// <returns></returns>
        private GenericPropertyWrapper GetPropertyWrapperForPropertyType(
            cms.businesslogic.propertytype.PropertyType pt,
            cms.businesslogic.datatype.DataTypeDefinition[] allDtds,
            out cms.businesslogic.datatype.DataTypeDefinition[] filteredDefinitions)
        {
            filteredDefinitions = allDtds;
            
            //not editable if any of the built in member types
            if (_contentType.ContentTypeItem is IMemberType)
            {
                var builtInAliases = global::Umbraco.Core.Constants.Conventions.Member.GetStandardPropertyTypeStubs().Select(x => x.Key).ToArray();
                var gpw = new GenericPropertyWrapper(builtInAliases.Contains(pt.Alias) == false);
                return gpw;
            }

            //not editable if prefixed with the special internal prefix
            if (pt.Alias.StartsWith(Constants.PropertyEditors.InternalGenericPropertiesPrefix))
            {
                var gpw = new GenericPropertyWrapper(false);
                return gpw;
            }

            //the rest are editable
            return new GenericPropertyWrapper();
        }

        private string GetJavaScriptForPropertySorting(string propSortClientId)
        {
            return @"(function($) {
                        var propSortId = ""#" + propSortClientId + @""";
                        $(document).ready(function() {
                            $(propSortId).next("".genericPropertyList"").sortable({
                                containment: '#tabs-container', 
                                connectWith: '.genericPropertyList', 
                                cancel: 'li.no-properties-on-tab, .propertyForm div[id^=""editbody""]',
                                tolerance: 'pointer',
                                start: function() {
                                    $('#tabs-container').addClass('doc-type-property-drop-zone');
                                },
                                stop: function() {
                                    $('#tabs-container').removeClass('doc-type-property-drop-zone');
                                },
                                update: function(event, ui) { 

                                    // Save new sort details for tab
                                    $(propSortId).val($(this).sortable('serialize'));

                                    // Handle move to new tab
                                    // - find tab name
                                    var tabName = $(this).siblings('h2').attr('data-tabname');

                                    // - find tab drop-down for item and set option selected that matches tab name
                                    var tabDropDownList = $(""select[name$='ddlTab']"", ui.item);
                                    $('option', tabDropDownList).each(function() {
                                        if ($(this).text() == tabName) {
                                            $(this).attr('selected', 'selected');            
                                        }                        
                                    });

                                    // Remove any no properties messages for tabs that now have a property
                                    $('li.no-properties-on-tab', $(this)).remove();

                                    // Add a no properties message for tabs that now have no properties
                                    $('#tabs-container ul.genericPropertyList:not(:has(li))').append('" + GetHtmlForNoPropertiesMessageListItem() + @"');
                                    
                                }});
                        });
                    })(jQuery);";
        }

        private void AddNoPropertiesDefinedMessage()
        {
            // Create no properties message as a ul in order to allow dragging of properties to it from other tabs
            PropertyTypes.Controls.Add(new LiteralControl("<ul class=\"genericPropertyList\">" + GetHtmlForNoPropertiesMessageListItem() + "</ul>"));
        }

        private string GetHtmlForNoPropertiesMessageListItem()
        {
            return @"<li class=""no-properties-on-tab"">" + ui.Text("settings", "noPropertiesDefinedOnTab", Security.CurrentUser) + "</li></ul>";
        }

        private void SavePropertyType(SaveClickEventArgs e, IContentTypeComposition contentTypeItem)
        {
            this.CreateChildControls();

            //The GenericPropertyWrapper control, which contains the details for the PropertyType being added
            GenericProperty gpData = gp.GenricPropertyControl;
            if (string.IsNullOrEmpty(gpData.Name.Trim()) == false && string.IsNullOrEmpty(gpData.Alias.Trim()) == false)
            {
                // when creating a property don't do anything special, propertyType.Alias will take care of it
                // don't enforce camel here because the user might have changed what the CoreStringsController returned
                var propertyTypeAlias = gpData.Alias;
                if (contentTypeItem.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    //Find the DataTypeDefinition that the PropertyType should be based on
                    var dataTypeDefinition = ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionById(gpData.Type);
                    var propertyType = new PropertyType(dataTypeDefinition)
                                           {
                                               Alias = propertyTypeAlias,
                                               Name = gpData.Name.Trim(),
                                               Mandatory = gpData.Mandatory,
                                               ValidationRegExp = gpData.Validation,
                                               Description = gpData.Description
                                           };
                    //gpData.Tab == 0 Generic Properties / No Group
                    if (gpData.Tab == 0)
                    {
                        contentTypeItem.AddPropertyType(propertyType);
                    }
                    else
                    {
                        //Find the PropertyGroup by its Id and then set the PropertyType on that group
                        var exists = contentTypeItem.CompositionPropertyGroups.Any(x => x.Id == gpData.Tab);
                        if (exists)
                        {
                            var propertyGroup = contentTypeItem.CompositionPropertyGroups.First(x => x.Id == gpData.Tab);
                            contentTypeItem.AddPropertyType(propertyType, propertyGroup.Name);
                        }
                        else
                        {
                            var tab = gpData.Tabs.FirstOrDefault(x => x.Id == gpData.Tab);
                            if (tab != null)
                            {
                                var caption = tab.GetRawCaption();
                                contentTypeItem.AddPropertyType(propertyType, caption);
                            }
                        }
                    }
                    gpData.Clear();
                }
                else
                {
                    e.Message = ui.Text("contentTypeDublicatePropertyType", Security.CurrentUser);
                    e.IconType = BasePage.speechBubbleIcon.warning;
                }
            }
        }

        private void UpdatePropertyTypes(IContentTypeComposition contentTypeItem)
        {
            //Loop through the _genericProperties ArrayList and update all existing PropertyTypes
            foreach (GenericPropertyWrapper gpw in _genericProperties)
            {
                if (gpw.PropertyType == null) continue;
                if (contentTypeItem.PropertyTypes == null || contentTypeItem.PropertyTypes.Any(x => x.Alias == gpw.PropertyType.Alias) == false) continue;
                var propertyType = contentTypeItem.PropertyTypes.First(x => x.Alias == gpw.PropertyType.Alias);
                if (propertyType == null) continue;
                var dataTypeDefinition = ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionById(gpw.GenricPropertyControl.Type);
                // when saving, respect user's casing, so do nothing here as propertyType takes care of it
                propertyType.Alias = gpw.GenricPropertyControl.Alias;
                propertyType.Name = gpw.GenricPropertyControl.Name;
                propertyType.Description = gpw.GenricPropertyControl.Description;
                propertyType.ValidationRegExp = gpw.GenricPropertyControl.Validation;
                propertyType.Mandatory = gpw.GenricPropertyControl.Mandatory;
                propertyType.DataTypeDatabaseType = dataTypeDefinition.DatabaseType;
                propertyType.DataTypeDefinitionId = dataTypeDefinition.Id;
                propertyType.PropertyEditorAlias = dataTypeDefinition.PropertyEditorAlias;

                if (propertyType.PropertyGroupId == null || propertyType.PropertyGroupId.Value != gpw.GenricPropertyControl.Tab)
                {
                    if (gpw.GenricPropertyControl.Tab == 0)
                    {
                        propertyType.PropertyGroupId = new Lazy<int>(() => 0);
                    }
                    else if (contentTypeItem.PropertyGroups.Any(x => x.Id == gpw.GenricPropertyControl.Tab))
                    {
                        propertyType.PropertyGroupId = new Lazy<int>(() => gpw.GenricPropertyControl.Tab);
                    }
                    else if (contentTypeItem.PropertyGroups.Any(x => x.ParentId == gpw.GenricPropertyControl.Tab))
                    {
                        var propertyGroup = contentTypeItem.PropertyGroups.First(x => x.ParentId == gpw.GenricPropertyControl.Tab);
                        propertyType.PropertyGroupId = new Lazy<int>(() => propertyGroup.Id);
                    }
                    else
                    {
                        var propertyGroup = contentTypeItem.CompositionPropertyGroups.First(x => x.Id == gpw.GenricPropertyControl.Tab);
                        contentTypeItem.AddPropertyGroup(propertyGroup.Name);
                        contentTypeItem.MovePropertyType(propertyType.Alias, propertyGroup.Name);
                    }
                }
            }

            //Update the SortOrder of the PropertyTypes
            foreach (HtmlInputHidden propSorter in _sortLists)
            {
                if (propSorter.Value.Trim() != "")
                {
                    string[] newSortOrders = propSorter.Value.Split("&".ToCharArray());
                    for (int i = 0; i < newSortOrders.Length; i++)
                    {
                        var propertyTypeId = int.Parse(newSortOrders[i].Split("=".ToCharArray())[1]);
                        if (contentTypeItem.PropertyTypes != null &&
                            contentTypeItem.PropertyTypes.Any(x => x.Id == propertyTypeId))
                        {
                            var propertyType = contentTypeItem.PropertyTypes.First(x => x.Id == propertyTypeId);
                            propertyType.SortOrder = i;
                        }
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

        /// <summary>
        /// Called asynchronously in order to delete a content type property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="cb"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private IAsyncResult BeginAsyncDeleteOperation(object sender, EventArgs e, AsyncCallback cb, object state)
        {
            Trace.Write("ContentTypeControlNew", "Start async operation");

            //get the args from the async state
            var args = (DeleteAsyncState)state;

            //start the task
            var result = _asyncDeleteTask.BeginInvoke(args, cb, args);
            return result;
        }

        /// <summary>
        /// Occurs once the async database delete operation has completed
        /// </summary>
        /// <param name="ar"></param>
        /// <remarks>
        /// This updates the UI elements
        /// </remarks>
        private void EndAsyncDeleteOperation(IAsyncResult ar)
        {
            Trace.Write("ContentTypeControlNew", "ending async operation");
            
            // reload content type (due to caching)
            LoadContentType(_contentType.Id);
            BindDataGenericProperties(true);
            
            Trace.Write("ContentTypeControlNew", "async operation ended");

            //complete it
            _asyncDeleteTask.EndInvoke(ar);
        }

        /// <summary>
        /// Removes a PropertyType from the current ContentType when user clicks "red x"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gpw_Delete(object sender, EventArgs e)
        {
            var state = new DeleteAsyncState(
                UmbracoContext,
                (GenericPropertyWrapper)sender);

            //Add the async operation to the page
            //NOTE: Must pass in a null and do not pass in a true to the 'executeInParallel', this is changed in .net 4.5 for the better, otherwise you'll get a ysod. 
            Page.RegisterAsyncTask(new PageAsyncTask(BeginAsyncDeleteOperation, EndAsyncDeleteOperation, null, state));

            //create the save task to be executed async
            _asyncDeleteTask = asyncState =>
                {
                    Trace.Write("ContentTypeControlNew", "executing task");

                    //we need to re-set the UmbracoContext since it will be nulled and our cache handlers need it
                    global::Umbraco.Web.UmbracoContext.Current = asyncState.UmbracoContext;

                    //if (_contentType.ContentTypeItem is IContentType 
                    //    || _contentType.ContentTypeItem is IMediaType
                    //    || _contentType.ContentTypeItem is IMemberType)
                    //{
                        _contentType.ContentTypeItem.RemovePropertyType(asyncState.GenericPropertyWrapper.PropertyType.Alias);
                        _contentType.Save();
                    //}
                    //else
                    //{
                    //    //if it is not a document type or a media type, then continue to call the legacy delete() method.
                    //    //the new API for document type and media type's will ensure that the data is removed correctly and that
                    //    //the cache is flushed correctly (using events).  If it is not one of these types, we'll rever to the 
                    //    //legacy operation (... like for members i suppose ?)
                    //    asyncState.GenericPropertyWrapper.GenricPropertyControl.PropertyType.delete();

                    //}

                    Trace.Write("ContentTypeControlNew", "task completing");
                };

            //execute the async tasks
            Page.ExecuteRegisteredAsyncTasks();
        }

        #endregion

        #region "Tab" Pane

        private void SetupTabPane()
        {
            uicontrols.TabPage tp = TabView1.NewTabPage("Tabs");
            tp.Controls.Add(pnlTab);
            BindTabs();
        }

        private IEnumerable<Tuple<int, string, int>> SaveTabs()
        {
            var tabs = new List<Tuple<int, string, int>>();//TabId, TabName, TabSortOrder
            foreach (DataGridItem dgi in dgTabs.Items)
            {
                int tabid = int.Parse(dgi.Cells[0].Text);
                string tabName = ((TextBox) dgi.FindControl("txtTab")).Text;
                int tabSortOrder;
                if (Int32.TryParse(((TextBox)dgi.FindControl("txtSortOrder")).Text, out tabSortOrder))
                {
                    tabs.Add(new Tuple<int, string, int>(tabid, tabName, tabSortOrder));
                }
            }
            return tabs;
        }

        private void BindTabs()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("name");
            dt.Columns.Add("id");
            dt.Columns.Add("order");

            foreach (var grp in _contentType.PropertyTypeGroups.OrderBy(p => p.SortOrder))
            {
                if (grp.ContentTypeId == _contentType.Id && grp.ParentId == 0)
                {
                    DataRow dr = dt.NewRow();
                    dr["name"] = grp.Name;
                    dr["id"] = grp.Id;
                    dr["order"] = grp.SortOrder;
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

        public DataTable DataTypeTable
        {
            get
            {
                if (_dataTypeTable == null)
                {
                    _dataTypeTable = new DataTable();
                    _dataTypeTable.Columns.Add("name");
                    _dataTypeTable.Columns.Add("id");

                    foreach (var dataType in cms.businesslogic.datatype.DataTypeDefinition.GetAll())
                    {
                        DataRow dr = _dataTypeTable.NewRow();
                        dr["name"] = dataType.Text;
                        dr["id"] = dataType.Id.ToString();
                        _dataTypeTable.Rows.Add(dr);
                    }
                }
                return _dataTypeTable;
            }
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

        /// <summary>
        /// Adds a new Tab to current ContentType when user clicks 'New Tab'-button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnNewTab_Click(object sender, EventArgs e)
        {
            if (txtNewTab.Text.Trim() != "")
            {
                //if (_contentType.ContentTypeItem is IContentType 
                //    || _contentType.ContentTypeItem is IMediaType
                //    || _contentType.ContentTypeItem is IMemberType)
                //{
                    _contentType.ContentTypeItem.AddPropertyGroup(txtNewTab.Text);
                    _contentType.Save();
                //}
                //else
                //{
                //    _contentType.AddVirtualTab(txtNewTab.Text);
                //}

                LoadContentType();

                var ea = new SaveClickEventArgs(ui.Text("contentTypeTabCreated", Security.CurrentUser));
                ea.IconType = BasePage.speechBubbleIcon.success;

                RaiseBubbleEvent(new object(), ea);

                txtNewTab.Text = "";

                BindTabs();
                BindDataGenericProperties(true);
            }

        }

        /// <summary>
        /// Removes a Tab from current ContentType when user clicks Delete button
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        protected void dgTabs_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                int propertyGroupId = int.Parse(e.Item.Cells[0].Text);
                //if (_contentType.ContentTypeItem is IContentType 
                //    || _contentType.ContentTypeItem is IMediaType
                //    || _contentType.ContentTypeItem is IMemberType)
                //{
                    var propertyGroup = _contentType.ContentTypeItem.PropertyGroups.FirstOrDefault(x => x.Id == propertyGroupId);
                    if (propertyGroup != null && string.IsNullOrEmpty(propertyGroup.Name) == false)
                    {
                        _contentType.ContentTypeItem.PropertyGroups.Remove(propertyGroup.Name);
                        _contentType.Save();
                    }
                //}

                _contentType.DeleteVirtualTab(propertyGroupId);

                LoadContentType();

                var ea = new SaveClickEventArgs(ui.Text("contentTypeTabDeleted", Security.CurrentUser));
                ea.IconType = BasePage.speechBubbleIcon.success;

                RaiseBubbleEvent(new object(), ea);

            }

            BindTabs();
            BindDataGenericProperties(true);
        }

        protected void dgTabs_itemdatabound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                ((DropDownList)e.Item.FindControl("dllTab")).SelectedValue =
                    ((DataRowView)e.Item.DataItem).Row["propertyTypeGroupId"].ToString();
                ((DropDownList)e.Item.FindControl("ddlType")).SelectedValue =
                    ((DataRowView)e.Item.DataItem).Row["type"].ToString();
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

        protected global::umbraco.uicontrols.PropertyPanel pp_isContainer;
        protected global::System.Web.UI.WebControls.CheckBox cb_isContainer;    

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
        protected global::System.Web.UI.WebControls.HiddenField tb_icon;
        protected global::System.Web.UI.WebControls.Literal lt_icon;

            
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
        /// Pane6 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane Pane6;

        /// <summary>
        /// pp_Root control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_Root;

        /// <summary>
        /// allowAtRoot control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBox allowAtRoot;

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
        /// checkTxtAliasJs control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal checkTxtAliasJs;

        /// <summary>
        /// DuplicateAliasValidator control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CustomValidator DuplicateAliasValidator;

        /// <summary>
        /// Pane9 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane Pane9;

        /// <summary>
        /// pp_compositions control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_compositions;

        /// <summary>
        /// lstContentTypeCompositions control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBoxList lstContentTypeCompositions;

        /// <summary>
        /// PlaceHolderContentTypeCompositions control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder PlaceHolderContentTypeCompositions;
    }
}