using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using umbraco.BasePages;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.uicontrols;
using Content = umbraco.cms.businesslogic.Content;
using ContentType = umbraco.cms.businesslogic.ContentType;
using Media = umbraco.cms.businesslogic.media.Media;
using Property = umbraco.cms.businesslogic.property.Property;
using StylesheetProperty = umbraco.cms.businesslogic.web.StylesheetProperty;

namespace umbraco.controls
{
    public class ContentControlLoadEventArgs : CancelEventArgs { }

    /// <summary>
    /// Summary description for ContentControl.
    /// </summary>
    public class ContentControl : TabView
    {

        
        internal Dictionary<string, IDataType> DataTypes = new Dictionary<string, IDataType>();
        private readonly Content _content;
        private UmbracoEnsuredPage _prntpage;
        public event EventHandler SaveAndPublish;
        public event EventHandler SaveToPublish;
        public event EventHandler Save;
        private readonly publishModes _canPublish = publishModes.NoPublish;
        public TabPage tpProp;
        public bool DoesPublish = false;
        public TextBox NameTxt = new TextBox();
        public PlaceHolder NameTxtHolder = new PlaceHolder();
        public RequiredFieldValidator NameTxtValidator = new RequiredFieldValidator();
        private readonly CustomValidator _nameTxtCustomValidator = new CustomValidator();
        private static readonly string UmbracoPath = SystemDirectories.Umbraco;
        public Pane PropertiesPane = new Pane();
        // zb-00036 #29889 : load it only once
        List<ContentType.TabI> _virtualTabs;
        //default to true!
        private bool _savePropertyDataWhenInvalid = true;
        private ContentType _contentType;

        
        public Content ContentObject
        {
            get { return _content; }
        }

        /// <summary>
        /// This property controls whether the content property values are persisted even if validation 
        /// fails. If set to false, then the values will not be persisted.
        /// </summary>
        /// <remarks>
        /// This is required because when we are editing content we should be persisting invalid values to the database
        /// as this makes it easier for editors to come back and fix up their changes before they publish. Of course we
        /// don't publish if the page is invalid. In the case of media and members, we don't want to persist the values
        /// to the database when the page is invalid because there is no published state.
        /// Relates to: http://issues.umbraco.org/issue/U4-227
        /// </remarks>
        public bool SavePropertyDataWhenInvalid
        {
            get { return _savePropertyDataWhenInvalid; }
            set { _savePropertyDataWhenInvalid = value; }
        }

        [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
        private string _errorMessage = "";

        [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
        public string ErrorMessage
        {
            set { _errorMessage = value; }
        }

        [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
        protected void standardSaveAndPublishHandler(object sender, EventArgs e)
        {
        }

        
        /// <summary>
        /// Constructor to set default properties.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="CanPublish"></param>
        /// <param name="Id"></param>
        /// <remarks>
        /// This method used to create all of the child controls too which is BAD since
        /// the page hasn't started initializing yet. Control IDs were not being named
        /// correctly, etc... I've moved the child control setup/creation to the CreateChildControls
        /// method where they are suposed to be.
        /// </remarks>
        public ContentControl(Content c, publishModes CanPublish, string Id)
        {
            ID = Id;
            this._canPublish = CanPublish;
            _content = c;

            Width = 350;
            Height = 350;

            _prntpage = (UmbracoEnsuredPage)Page;

            // zb-00036 #29889 : load it only once
            if (_virtualTabs == null)
                _virtualTabs = _content.ContentType.getVirtualTabs.ToList();

            foreach (ContentType.TabI t in _virtualTabs)
            {
                TabPage tp = NewTabPage(t.Caption);
                AddSaveAndPublishButtons(ref tp);
            }
        }

        /// <summary>
        /// Create and setup all of the controls child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            
            _prntpage = (UmbracoEnsuredPage)Page;
            int i = 0;
            Hashtable inTab = new Hashtable();

            // zb-00036 #29889 : load it only once
            if (_virtualTabs == null)
                _virtualTabs = _content.ContentType.getVirtualTabs.ToList();

            if(_contentType == null)
                _contentType = ContentType.GetContentType(_content.ContentType.Id);

            foreach (ContentType.TabI tab in _virtualTabs)
            {
                var tabPage = this.Panels[i] as TabPage;
                if (tabPage == null)
                {
                    throw new ArgumentException("Unable to load tab \"" + tab.Caption + "\"");
                }

                tabPage.Style.Add("text-align", "center");

                //Legacy vs New API loading of PropertyTypes
                if (_contentType.ContentTypeItem != null)
                {
                    LoadPropertyTypes(_contentType.ContentTypeItem, tabPage, inTab, tab.Id, tab.Caption);
                }
                else
                {
                    LoadPropertyTypes(tab, tabPage, inTab);
                }

                i++;
            }

            // Add property pane
            tpProp = NewTabPage(ui.Text("general", "properties"));
            AddSaveAndPublishButtons(ref tpProp);
            tpProp.Controls.Add(
                new LiteralControl("<div id=\"errorPane_" + tpProp.ClientID +
                                   "\" style=\"display: none; text-align: left; color: red;width: 100%; border: 1px solid red; background-color: #FCDEDE\"><div><b>There were errors - data has not been saved!</b><br/></div></div>"));

            //if the property is not in a tab, add it to the general tab
            var props = _content.GenericProperties;
            foreach (Property p in props.OrderBy(x => x.PropertyType.SortOrder))
            {
                if (inTab[p.PropertyType.Id.ToString()] == null)
                    AddControlNew(p, tpProp, ui.Text("general", "properties"));
            }

        }

        /// <summary>
        /// Loades PropertyTypes by Tab/PropertyGroup using the new API.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="tabPage"></param>
        /// <param name="inTab"></param>
        /// <param name="tabId"></param>
        /// <param name="tabCaption"></param>
        private void LoadPropertyTypes(IContentTypeComposition contentType, TabPage tabPage, Hashtable inTab, int tabId, string tabCaption)
        {
            var groups = contentType.CompositionPropertyGroups.Where(x => x.Name == tabCaption);
            var propertyTypes = groups
                .SelectMany(x => x.PropertyTypes)
                .OrderBy(x => x.SortOrder)
                .Select(x => Tuple.Create(x.Id, x.Alias));

            foreach (var items in propertyTypes)
            {
                var property = _content.getProperty(items.Item2);
                if (property != null)
                {
                    AddControlNew(property, tabPage, tabCaption);

                    if (!inTab.ContainsKey(items.Item1.ToString(CultureInfo.InvariantCulture)))
                        inTab.Add(items.Item1.ToString(CultureInfo.InvariantCulture), true);
                }
                else
                {
                    throw new ArgumentNullException(
                        string.Format(
                            "Property {0} ({1}) on Content Type {2} could not be retrieved for Document {3} on Tab Page {4}. To fix this problem, delete the property and recreate it.",
                            items.Item2, items.Item1, _content.ContentType.Alias, _content.Id,
                            tabCaption));
                }
            }
        }

        /// <summary>
        /// Loades PropertyTypes by Tab using the Legacy API.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="tabPage"></param>
        /// <param name="inTab"></param>
        private void LoadPropertyTypes(ContentType.TabI tab, TabPage tabPage, Hashtable inTab)
        {
            // Iterate through the property types and add them to the tab
            // zb-00036 #29889 : fix property types getter to get the right set of properties
            // ge : had a bit of a corrupt db and got weird NRE errors so rewrote this to catch the error and rethrow with detail
            var propertyTypes = tab.GetPropertyTypes(_content.ContentType.Id);
            foreach (var propertyType in propertyTypes.OrderBy(x => x.SortOrder))
            {
                var property = _content.getProperty(propertyType);
                if (property != null && tabPage != null)
                {
                    AddControlNew(property, tabPage, tab.Caption);

                    // adding this check, as we occasionally get an already in dictionary error, though not sure why
                    if (!inTab.ContainsKey(propertyType.Id.ToString(CultureInfo.InvariantCulture)))
                        inTab.Add(propertyType.Id.ToString(CultureInfo.InvariantCulture), true);
                }
                else
                {
                    throw new ArgumentNullException(
                        string.Format(
                            "Property {0} ({1}) on Content Type {2} could not be retrieved for Document {3} on Tab Page {4}. To fix this problem, delete the property and recreate it.",
                            propertyType.Alias, propertyType.Id, _content.ContentType.Alias, _content.Id, tab.Caption));
                }
            }
        }

        /// <summary>
        /// Initializes the control and ensures child controls are setup
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            EnsureChildControls();

            // Add extras for the property tabpage. .
            ContentControlLoadEventArgs contentcontrolEvent = new ContentControlLoadEventArgs();
            FireBeforeContentControlLoad(contentcontrolEvent);

            if (!contentcontrolEvent.Cancel)
            {

                NameTxt.ID = "NameTxt";
                if (!Page.IsPostBack)
                {
                    NameTxt.Text = _content.Text;
                }

                // Name validation
                NameTxtValidator.ControlToValidate = NameTxt.ID;
                _nameTxtCustomValidator.ControlToValidate = NameTxt.ID;
                string[] errorVars = { ui.Text("name") };
                NameTxtValidator.ErrorMessage = " " + ui.Text("errorHandling", "errorMandatoryWithoutTab", errorVars) + "<br/>";
                NameTxtValidator.EnableClientScript = false;
                NameTxtValidator.Display = ValidatorDisplay.Dynamic;                
                _nameTxtCustomValidator.EnableClientScript = false;
                _nameTxtCustomValidator.Display = ValidatorDisplay.Dynamic;
                _nameTxtCustomValidator.ServerValidate += NameTxtCustomValidatorServerValidate;
                _nameTxtCustomValidator.ValidateEmptyText = false;

                NameTxtHolder.Controls.Add(NameTxt);
                NameTxtHolder.Controls.Add(NameTxtValidator);
                NameTxtHolder.Controls.Add(_nameTxtCustomValidator);
                PropertiesPane.addProperty(ui.Text("general", "name"), NameTxtHolder);

                Literal ltt = new Literal();
                ltt.Text = _content.User.Name;
                PropertiesPane.addProperty(ui.Text("content", "createBy"), ltt);

                ltt = new Literal();
                ltt.Text = _content.CreateDateTime.ToString();
                PropertiesPane.addProperty(ui.Text("content", "createDate"), ltt);

                ltt = new Literal();
                ltt.Text = _content.Id.ToString();
                PropertiesPane.addProperty("Id", ltt);

                if (_content is Media)
                {
                    PropertiesPane.addProperty(ui.Text("content", "mediatype"), new LiteralControl(_content.ContentType.Alias));
                }

                tpProp.Controls.AddAt(0, PropertiesPane);
                tpProp.Style.Add("text-align", "center");
            }
        }

        /// <summary>
        /// Custom validates the content name field
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// We need to ensure people are not entering XSS attacks on this field
        /// http://issues.umbraco.org/issue/U4-485
        /// 
        /// This doesn't actually 'validate' but changes the text field value and strips html
        /// </remarks>
        void NameTxtCustomValidatorServerValidate(object source, ServerValidateEventArgs args)
        {
            NameTxt.Text = NameTxt.Text.StripHtml();
            args.IsValid = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ContentControlLoadEventArgs contentcontrolEvent = new ContentControlLoadEventArgs();
            FireAfterContentControlLoad(contentcontrolEvent);
        }

        /// <summary>
        /// Sets the name (text) and values on the data types of the document
        /// </summary>
        private void SetNameAndDataTypeValues()
        {
            //we only continue saving anything if: 
            // SavePropertyDataWhenInvalid == true
            // OR if the page is actually valid.
            if (SavePropertyDataWhenInvalid || Page.IsValid)
            {

                foreach (var property in DataTypes)
                {
                    var defaultData = property.Value.Data as DefaultData;
                    if (defaultData != null)
                    {
                        defaultData.PropertyTypeAlias = property.Key;
                        defaultData.NodeId = _content.Id;
                    }
                    property.Value.DataEditor.Save();
                }

                //don't update if the name is empty
                if (!NameTxt.Text.IsNullOrWhiteSpace())
                {
                    _content.Text = NameTxt.Text;
                }
            }
        }

        private void SaveClick(object sender, ImageClickEventArgs e)
        {
            SetNameAndDataTypeValues();

            if (Save != null)
            {
                Save(this, new EventArgs());
            }
        }

        private void DoSaveAndPublish(object sender, ImageClickEventArgs e)
        {
            DoesPublish = true;

            SetNameAndDataTypeValues();

            //NOTE: This is only here to keep backwards compatibility.
            // see: http://issues.umbraco.org/issue/U4-1660
            Save(this, new EventArgs());

            if (SaveAndPublish != null)
            {
                SaveAndPublish(this, new EventArgs());    
            }
        }

        private void DoSaveToPublish(object sender, ImageClickEventArgs e)
        {
            SaveClick(sender, e);
            if (SaveToPublish != null)
            {
                SaveToPublish(this, new EventArgs());
            }
        }

        private void AddSaveAndPublishButtons(ref TabPage tp)
        {
            MenuImageButton menuSave = tp.Menu.NewImageButton();
            menuSave.ID = tp.ID + "_save";
            menuSave.ImageUrl = UmbracoPath + "/images/editor/save.gif";
            menuSave.Click += new ImageClickEventHandler(SaveClick);
            menuSave.OnClickCommand = "invokeSaveHandlers();";
            menuSave.AltText = ui.Text("buttons", "save");
            if (_canPublish == publishModes.Publish)
            {
                MenuImageButton menuPublish = tp.Menu.NewImageButton();
                menuPublish.ID = tp.ID + "_publish";
                menuPublish.ImageUrl = UmbracoPath + "/images/editor/saveAndPublish.gif";
                menuPublish.OnClickCommand = "invokeSaveHandlers();";
                menuPublish.Click += new ImageClickEventHandler(DoSaveAndPublish);
                menuPublish.AltText = ui.Text("buttons", "saveAndPublish");
            }
            else if (_canPublish == publishModes.SendToPublish)
            {
                MenuImageButton menuToPublish = tp.Menu.NewImageButton();
                menuToPublish.ID = tp.ID + "_topublish";
                menuToPublish.ImageUrl = UmbracoPath + "/images/editor/saveToPublish.gif";
                menuToPublish.OnClickCommand = "invokeSaveHandlers();";
                menuToPublish.Click += new ImageClickEventHandler(DoSaveToPublish);
                menuToPublish.AltText = ui.Text("buttons", "saveToPublish");
            }
        }


        private void AddControlNew(Property p, TabPage tp, string cap)
        {
            IDataType dt = p.PropertyType.DataTypeDefinition.DataType;

            //check that property editor has been set for the data type used by this property
            if (dt != null)
            {
                dt.DataEditor.Editor.ID = string.Format("prop_{0}", p.PropertyType.Alias);

                dt.Data.PropertyId = p.Id;

                //Add the DataType to an internal dictionary, which will be used to call the save method on the IDataEditor
                //and to retrieve the value from IData in editContent.aspx.cs, so that it can be set on the legacy Document class.
                DataTypes.Add(p.PropertyType.Alias, dt);

                // check for buttons
                IDataFieldWithButtons df1 = dt.DataEditor.Editor as IDataFieldWithButtons;
                if (df1 != null)
                {
                    ((Control)df1).ID = p.PropertyType.Alias;


                    if (df1.MenuIcons.Length > 0)
                        tp.Menu.InsertSplitter();


                    // Add buttons
                    int c = 0;
                    bool atEditHtml = false;
                    bool atSplitter = false;
                    foreach (object o in df1.MenuIcons)
                    {
                        try
                        {
                            MenuIconI m = (MenuIconI)o;
                            MenuIconI mi = tp.Menu.NewIcon();
                            mi.ImageURL = m.ImageURL;
                            mi.OnClickCommand = m.OnClickCommand;
                            mi.AltText = m.AltText;
                            mi.ID = tp.ID + "_" + m.ID;

                            if (m.ID == "html")
                                atEditHtml = true;
                            else
                                atEditHtml = false;

                            atSplitter = false;
                        }
                        catch
                        {
                            tp.Menu.InsertSplitter();
                            atSplitter = true;
                        }

                        // Testing custom styles in editor
                        if (atSplitter && atEditHtml && dt.DataEditor.TreatAsRichTextEditor)
                        {
                            DropDownList ddl = tp.Menu.NewDropDownList();

                            ddl.Style.Add("margin-bottom", "5px");
                        ddl.Items.Add(ui.Text("buttons", "styleChoose"));
                            ddl.ID = tp.ID + "_editorStyle";
                            if (StyleSheet.GetAll().Length > 0)
                            {
                                foreach (StyleSheet s in StyleSheet.GetAll())
                                {
                                    foreach (StylesheetProperty sp in s.Properties)
                                    {
                                        ddl.Items.Add(new ListItem(sp.Text, sp.Alias));
                                    }
                                }
                            }
                            ddl.Attributes.Add("onChange", "addStyle(this, '" + p.PropertyType.Alias + "');");
                            atEditHtml = false;
                        }
                        c++;
                    }
                }

                // check for element additions
                IMenuElement menuElement = dt.DataEditor.Editor as IMenuElement;
                if (menuElement != null)
                {
                    // add separator
                    tp.Menu.InsertSplitter();

                    // add the element
                    tp.Menu.NewElement(menuElement.ElementName, menuElement.ElementIdPreFix + p.Id.ToString(),
                                       menuElement.ElementClass, menuElement.ExtraMenuWidth);
                }

                Pane pp = new Pane();
                Control holder = new Control();
                holder.Controls.Add(dt.DataEditor.Editor);
                if (p.PropertyType.DataTypeDefinition.DataType.DataEditor.ShowLabel)
                {
                    string caption = p.PropertyType.Name;
                    if (p.PropertyType.Description != null && p.PropertyType.Description != String.Empty)
                    switch (UmbracoConfig.For.UmbracoSettings().Content.PropertyContextHelpOption)
                        {
                            case "icon":
                                caption += " <img src=\"" + this.ResolveUrl(SystemDirectories.Umbraco) + "/images/help.png\" class=\"umbPropertyContextHelp\" alt=\"" + p.PropertyType.Description + "\" title=\"" + p.PropertyType.Description + "\" />";
                                break;
                            case "text":
                                caption += "<br /><small>" + umbraco.library.ReplaceLineBreaks(p.PropertyType.Description) + "</small>";
                                break;
                        }
                    pp.addProperty(caption, holder);
                }
                else
                    pp.addProperty(holder);

                // Validation
                if (p.PropertyType.Mandatory)
                {
                    try
                    {
                        var rq = new RequiredFieldValidator
                        {
                            ControlToValidate = dt.DataEditor.Editor.ID,
                            CssClass = "error"
                        };
                        rq.Style.Add(HtmlTextWriterStyle.Display, "block");
                        rq.Style.Add(HtmlTextWriterStyle.Padding, "2px");
                        var component = dt.DataEditor.Editor; // holder.FindControl(rq.ControlToValidate);
                        var attribute = (ValidationPropertyAttribute)TypeDescriptor.GetAttributes(component)[typeof(ValidationPropertyAttribute)];
                        PropertyDescriptor pd = null;
                        if (attribute != null)
                        {
                            pd = TypeDescriptor.GetProperties(component, null)[attribute.Name];
                        }
                        if (pd != null)
                        {
                            rq.EnableClientScript = false;
                            rq.Display = ValidatorDisplay.Dynamic;
                            string[] errorVars = { p.PropertyType.Name, cap };
                        rq.ErrorMessage = ui.Text("errorHandling", "errorMandatory", errorVars) + "<br/>";
                            holder.Controls.AddAt(0, rq);
                        }
                    }
                    catch (Exception valE)
                    {
                        HttpContext.Current.Trace.Warn("contentControl",
                                                       "EditorControl (" + dt.DataTypeName + ") does not support validation",
                                                       valE);
                    }
                }

                // RegExp Validation
                if (p.PropertyType.ValidationRegExp != "")
                {
                    try
                    {
                        var rv = new RegularExpressionValidator
                        {
                            ControlToValidate = dt.DataEditor.Editor.ID,
                            CssClass = "error"
                        };
                        rv.Style.Add(HtmlTextWriterStyle.Display, "block");
                        rv.Style.Add(HtmlTextWriterStyle.Padding, "2px");
                        var component = dt.DataEditor.Editor; // holder.FindControl(rq.ControlToValidate);
                        var attribute = (ValidationPropertyAttribute)TypeDescriptor.GetAttributes(component)[typeof(ValidationPropertyAttribute)];
                        PropertyDescriptor pd = null;
                        if (attribute != null)
                        {
                            pd = TypeDescriptor.GetProperties(component, null)[attribute.Name];
                        }
                        if (pd != null)
                        {
                            rv.ValidationExpression = p.PropertyType.ValidationRegExp;
                            rv.EnableClientScript = false;
                            rv.Display = ValidatorDisplay.Dynamic;
                            string[] errorVars = { p.PropertyType.Name, cap };
                        rv.ErrorMessage = ui.Text("errorHandling", "errorRegExp", errorVars) + "<br/>";
                            holder.Controls.AddAt(0, rv);
                        }
                    }
                    catch (Exception valE)
                    {
                        HttpContext.Current.Trace.Warn("contentControl",
                                                       "EditorControl (" + dt.DataTypeName + ") does not support validation",
                                                       valE);
                    }
                }

                // This is once again a nasty nasty hack to fix gui when rendering wysiwygeditor
                if (dt.DataEditor.TreatAsRichTextEditor)
                {
                    tp.Controls.Add(dt.DataEditor.Editor);
                }
                else
                {
                    Panel ph = new Panel();
                    ph.Attributes.Add("style", "padding: 0; position: relative;"); // NH 4.7.1, latest styles added to support CP item: 30363
                    ph.Controls.Add(pp);

                    tp.Controls.Add(ph);
                }
            }
            else
            {
                
                var ph = new Panel();

                var pp = new Pane();

                var missingPropertyEditorLabel = new Literal
                {
                    Text = ui.Text("errors", "missingPropertyEditorErrorMessage")
                };

                pp.addProperty(p.PropertyType.Name, missingPropertyEditorLabel);
                
                ph.Attributes.Add("style", "padding: 0; position: relative;");
                
                ph.Controls.Add(pp);

                tp.Controls.Add(ph);
            }

            
        }

        public enum publishModes
        {
            Publish,
            SendToPublish,
            NoPublish
        }

        // EVENTS
        public delegate void BeforeContentControlLoadEventHandler(ContentControl contentControl, ContentControlLoadEventArgs e);
        public delegate void AfterContentControlLoadEventHandler(ContentControl contentControl, ContentControlLoadEventArgs e);


        /// <summary>
        /// Occurs when [before content control load].
        /// </summary>
        public static event BeforeContentControlLoadEventHandler BeforeContentControlLoad;
        /// <summary>
        /// Fires the before content control load.
        /// </summary>
        /// <param name="e">The <see cref="umbraco.controls.ContentControlLoadEventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeContentControlLoad(ContentControlLoadEventArgs e)
        {
            if (BeforeContentControlLoad != null)
                BeforeContentControlLoad(this, e);
        }

        /// <summary>
        /// Occurs when [before content control load].
        /// </summary>
        public static event AfterContentControlLoadEventHandler AfterContentControlLoad;
        /// <summary>
        /// Fires the before content control load.
        /// </summary>
        /// <param name="e">The <see cref="umbraco.controls.ContentControlLoadEventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterContentControlLoad(ContentControlLoadEventArgs e)
        {
            if (AfterContentControlLoad != null)
                AfterContentControlLoad(this, e);
        }
    }
}
