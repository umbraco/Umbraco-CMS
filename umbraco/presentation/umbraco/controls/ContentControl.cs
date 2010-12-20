using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.uicontrols;
using Content = umbraco.cms.businesslogic.Content;
using System.Linq;
using umbraco.IO;

namespace umbraco.controls
{
    public class ContentControlLoadEventArgs : System.ComponentModel.CancelEventArgs { }

    /// <summary>
    /// Summary description for ContentControl.
    /// </summary>
    public class ContentControl : TabView
    {
        private Content _content;
        private ArrayList _dataFields = new ArrayList();
        private UmbracoEnsuredPage prntpage;
        public event EventHandler SaveAndPublish;
        public event EventHandler SaveToPublish;
        public event EventHandler Save;
        private publishModes CanPublish = publishModes.NoPublish;
        public TabPage tpProp;
        public bool DoesPublish = false;        
        public TextBox NameTxt = new TextBox();
        public PlaceHolder NameTxtHolder = new PlaceHolder();
        public RequiredFieldValidator NameTxtValidator = new RequiredFieldValidator();
        private static string _UmbracoPath = SystemDirectories.Umbraco;
        public Pane PropertiesPane = new Pane();

        public Content ContentObject
        {
            get { return _content; }
        }

        // Error messages
        private string _errorMessage = "";

        public string ErrorMessage
        {
            set { _errorMessage = value; }
        }

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
            this.CanPublish = CanPublish;
            _content = c;

            Width = 350;
            Height = 350;

            SaveAndPublish += new EventHandler(standardSaveAndPublishHandler);
			Save += new EventHandler(standardSaveAndPublishHandler);
			prntpage = (UmbracoEnsuredPage)Page;

            foreach (ContentType.TabI t in _content.ContentType.getVirtualTabs.ToList())
            {
                TabPage tp = NewTabPage(t.Caption);
                addSaveAndPublishButtons(ref tp);
            }
        }

        /// <summary>
        /// Create and setup all of the controls child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

			SaveAndPublish += new EventHandler(standardSaveAndPublishHandler);
			Save += new EventHandler(standardSaveAndPublishHandler);
			prntpage = (UmbracoEnsuredPage)Page;
            int i = 0;
            var inTab = new Hashtable();

            foreach (ContentType.TabI t in _content.ContentType.getVirtualTabs.ToList())
			{
                var tp = this.Panels[i] as TabPage;
                if (tp == null)
                {
                    throw new ArgumentException("Unable to load tab \"" + t.Caption + "\"");
                }
				//TabPage tp = NewTabPage(t.Caption);
				//addSaveAndPublishButtons(ref tp);

				tp.Style.Add("text-align", "center");


				// Iterate through the property types and add them to the tab
				foreach (PropertyType pt in t.PropertyTypes)
				{
					// table.Rows.Add(addControl(_content.getProperty(editPropertyType.Alias), tp));
					addControlNew(_content.getProperty(pt), tp, t.Caption);
					inTab.Add(pt.Id.ToString(), true);
				}

                i++;
			}

			// Add property pane
			tpProp = NewTabPage(ui.Text("general", "properties", null));
			addSaveAndPublishButtons(ref tpProp);
			tpProp.Controls.Add(
				new LiteralControl("<div id=\"errorPane_" + tpProp.ClientID +
								   "\" style=\"display: none; text-align: left; color: red;width: 100%; border: 1px solid red; background-color: #FCDEDE\"><div><b>There were errors - data has not been saved!</b><br/></div></div>"));

            //if the property is not in a tab, add it to the general tab
            var props = _content.GenericProperties;
            foreach (Property p in props)
            {
                if (inTab[p.PropertyType.Id.ToString()] == null)
                    addControlNew(p, tpProp, ui.Text("general", "properties", null));
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
                string[] errorVars = { ui.Text("name") };
                NameTxtValidator.ErrorMessage = " " + ui.Text("errorHandling", "errorMandatoryWithoutTab", errorVars, null) + "<br/>";
                NameTxtValidator.EnableClientScript = false;
                NameTxtValidator.Display = ValidatorDisplay.Dynamic;
                NameTxtHolder.Controls.Add(NameTxt);
                NameTxtHolder.Controls.Add(NameTxtValidator);
                PropertiesPane.addProperty(ui.Text("general", "name", null), NameTxtHolder);

                Literal ltt = new Literal();
                ltt.Text = _content.User.Name;
                PropertiesPane.addProperty(ui.Text("content", "createBy", null), ltt);

                ltt = new Literal();
                ltt.Text = _content.CreateDateTime.ToString();
                PropertiesPane.addProperty(ui.Text("content", "createDate", null), ltt);

                ltt = new Literal();
                ltt.Text = _content.Id.ToString();
                PropertiesPane.addProperty("Id", ltt);

                tpProp.Controls.AddAt(0, PropertiesPane);
                tpProp.Style.Add("text-align", "center");
                //tpProp.Style.Add("padding", "10px");
            }
        }

        protected override void OnLoad(EventArgs e)
        {   
            base.OnLoad(e);

            ContentControlLoadEventArgs contentcontrolEvent = new ContentControlLoadEventArgs();
            FireAfterContentControlLoad(contentcontrolEvent);
        }


        private void saveClick(object Sender, ImageClickEventArgs e)
        {
            var doc = this._content as Document;
            if (doc != null)
            {
                var docArgs = new SaveEventArgs();
                doc.FireBeforeSave(docArgs);

                if (docArgs.Cancel) //TODO: need to have some notification to the user here
                {
                    return;
                }
            }
            foreach (IDataEditor df in _dataFields)
            {
                df.Save();
            }
            _content.Text = NameTxt.Text;

            Save(this, new EventArgs());
        }

        private void savePublish(object Sender, ImageClickEventArgs e)
        {
            DoesPublish = true;
            saveClick(Sender, e);

            SaveAndPublish(this, new EventArgs());
        }

        private void saveToPublish(object Sender, ImageClickEventArgs e)
        {
            saveClick(Sender, e);
            SaveToPublish(this, new EventArgs());
        }

        private void addSaveAndPublishButtons(ref TabPage tp)
        {
            MenuImageButton menuSave = tp.Menu.NewImageButton();
            menuSave.ID = tp.ID + "_save";
            menuSave.ImageUrl = _UmbracoPath + "/images/editor/save.gif";
            menuSave.Click += new ImageClickEventHandler(saveClick);
            menuSave.OnClickCommand = "invokeSaveHandlers();";
            menuSave.AltText = ui.Text("buttons", "save", null);
            if (CanPublish == publishModes.Publish)
            {
                MenuImageButton menuPublish = tp.Menu.NewImageButton();
                menuPublish.ID = tp.ID + "_publish";
                menuPublish.ImageUrl = _UmbracoPath + "/images/editor/saveAndPublish.gif";
                menuPublish.OnClickCommand = "invokeSaveHandlers();";
                menuPublish.Click += new ImageClickEventHandler(savePublish);
                menuPublish.AltText = ui.Text("buttons", "saveAndPublish", null);
            }
            else if (CanPublish == publishModes.SendToPublish)
            {
                MenuImageButton menuToPublish = tp.Menu.NewImageButton();
                menuToPublish.ID = tp.ID + "_topublish";
                menuToPublish.ImageUrl = _UmbracoPath + "/images/editor/saveToPublish.gif";
                menuToPublish.OnClickCommand = "invokeSaveHandlers();";
                menuToPublish.Click += new ImageClickEventHandler(saveToPublish);
                menuToPublish.AltText = ui.Text("buttons", "saveToPublish", null);
            }
        }


        private void addControlNew(Property p, TabPage tp, string Caption)
        {
            IDataType dt = p.PropertyType.DataTypeDefinition.DataType;
            dt.DataEditor.Editor.ID = string.Format("prop_{0}", p.PropertyType.Alias);
            dt.Data.PropertyId = p.Id;

            // check for buttons
            IDataFieldWithButtons df1 = dt.DataEditor.Editor as IDataFieldWithButtons;
            if (df1 != null)
            {
                // df1.Alias = p.PropertyType.Alias;
                /*
				// df1.Version = _content.Version;
				editDataType.Data.PropertyId = p.Id;
				*/
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
                        ddl.Items.Add(ui.Text("buttons", "styleChoose", null));
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


            // fieldData.Alias = p.PropertyType.Alias;
            // ((Control) fieldData).ID = p.PropertyType.Alias;
            // fieldData.Text = p.Value.ToString();

            _dataFields.Add(dt.DataEditor.Editor);


            Pane pp = new Pane();
            Control holder = new Control();
            holder.Controls.Add(dt.DataEditor.Editor);
            if (p.PropertyType.DataTypeDefinition.DataType.DataEditor.ShowLabel)
            {
                string caption = p.PropertyType.Name;
                if (p.PropertyType.Description != null && p.PropertyType.Description != String.Empty)
                    switch (UmbracoSettings.PropertyContextHelpOption)
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
                    RequiredFieldValidator rq = new RequiredFieldValidator();
                    rq.ControlToValidate = dt.DataEditor.Editor.ID;
                    Control component = dt.DataEditor.Editor; // holder.FindControl(rq.ControlToValidate);
                    ValidationPropertyAttribute attribute =
                        (ValidationPropertyAttribute)
                        TypeDescriptor.GetAttributes(component)[typeof(ValidationPropertyAttribute)];
                    PropertyDescriptor pd = null;
                    if (attribute != null)
                    {
                        pd = TypeDescriptor.GetProperties(component, (Attribute[])null)[attribute.Name];
                    }
                    if (pd != null)
                    {
                        rq.EnableClientScript = false;
                        rq.Display = ValidatorDisplay.Dynamic;
                        string[] errorVars = { p.PropertyType.Name, Caption };
                        rq.ErrorMessage = ui.Text("errorHandling", "errorMandatory", errorVars, null) + "<br/>";
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
                    RegularExpressionValidator rv = new RegularExpressionValidator();
                    rv.ControlToValidate = dt.DataEditor.Editor.ID;

                    Control component = dt.DataEditor.Editor; // holder.FindControl(rq.ControlToValidate);
                    ValidationPropertyAttribute attribute =
                        (ValidationPropertyAttribute)
                        TypeDescriptor.GetAttributes(component)[typeof(ValidationPropertyAttribute)];
                    PropertyDescriptor pd = null;
                    if (attribute != null)
                    {
                        pd = TypeDescriptor.GetProperties(component, (Attribute[])null)[attribute.Name];
                    }
                    if (pd != null)
                    {
                        rv.ValidationExpression = p.PropertyType.ValidationRegExp;
                        rv.EnableClientScript = false;
                        rv.Display = ValidatorDisplay.Dynamic;
                        string[] errorVars = { p.PropertyType.Name, Caption };
                        rv.ErrorMessage = ui.Text("errorHandling", "errorRegExp", errorVars, null) + "<br/>";
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
                ph.Attributes.Add("style", "padding: 0px 0px 0px 0px");
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

        private string dictinaryItem(string alias)
        {
            if (alias.Substring(1, 0) == "#")
            {

                if (Dictionary.DictionaryItem.hasKey(alias.Substring(1)))
                {

                    Dictionary.DictionaryItem di = new Dictionary.DictionaryItem(alias.Substring(1));

                    if (di != null && !string.IsNullOrEmpty(di.Value()))
                        return di.Value();
                }

            }

            return alias + " " + alias.Substring(1);
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
