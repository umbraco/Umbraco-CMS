using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.editorControls.tinymce;
using umbraco.editorControls.tinyMCE3.webcontrol;
using umbraco.editorControls.wysiwyg;
using umbraco.interfaces;
using Umbraco.Core.IO;
using umbraco.presentation;
using umbraco.uicontrols;

namespace umbraco.editorControls.tinyMCE3
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class TinyMCE : TinyMCEWebControl, IDataEditor, IMenuElement
    {
        private readonly string _activateButtons = "";
        private readonly string _advancedUsers = "";
        private readonly SortedList _buttons = new SortedList();
        private readonly IData _data;
        private readonly string _disableButtons = "help,visualaid,";
        private readonly string _editorButtons = "";
        private readonly bool _enableContextMenu;
        private readonly bool _fullWidth;
        private readonly int _height;
        private readonly SortedList _mceButtons = new SortedList();
        private readonly ArrayList _menuIcons = new ArrayList();
        private readonly bool _showLabel;
        private readonly ArrayList _stylesheets = new ArrayList();
        private readonly int _width;

        private readonly int m_maxImageWidth = 500;
        private bool _isInitialized;
        private string _plugins = "";

        public TinyMCE(IData Data, string Configuration)
        {
            _data = Data;
            try
            {
                string[] configSettings = Configuration.Split("|".ToCharArray());

                if (configSettings.Length > 0)
                {
                    _editorButtons = configSettings[0];

                    if (configSettings.Length > 1)
                        if (configSettings[1] == "1")
                            _enableContextMenu = true;

                    if (configSettings.Length > 2)
                        _advancedUsers = configSettings[2];

                    if (configSettings.Length > 3)
                    {
                        if (configSettings[3] == "1")
                            _fullWidth = true;
                        else if (configSettings[4].Split(',').Length > 1)
                        {
                            _width = int.Parse(configSettings[4].Split(',')[0]);
                            _height = int.Parse(configSettings[4].Split(',')[1]);
                        }
                    }

                    // default width/height
                    if (_width < 1)
                        _width = 500;
                    if (_height < 1)
                        _height = 400;

                    // add stylesheets
                    if (configSettings.Length > 4)
                    {
                        foreach (string s in configSettings[5].Split(",".ToCharArray()))
                            _stylesheets.Add(s);
                    }

                    if (configSettings.Length > 6 && configSettings[6] != "")
                        _showLabel = bool.Parse(configSettings[6]);

                    if (configSettings.Length > 7 && configSettings[7] != "")
                        m_maxImageWidth = int.Parse(configSettings[7]);

                    // sizing
                    if (!_fullWidth)
                    {
                        config.Add("width", _width.ToString());
                        config.Add("height", _height.ToString());
                    }

                    if (_enableContextMenu)
                        _plugins += ",contextmenu";


                    // If the editor is used in umbraco, use umbraco's own toolbar
                    bool onFront = false;
                    if (GlobalSettings.RequestIsInUmbracoApplication(HttpContext.Current))
                    {
                        config.Add("theme_umbraco_toolbar_location", "external");
                        config.Add("skin", "umbraco");
                        config.Add("inlinepopups_skin ", "umbraco");
                    }
                    else
                    {
                        onFront = true;
                        config.Add("theme_umbraco_toolbar_location", "top");
                    }

                    // load plugins
                    IDictionaryEnumerator pluginEnum = tinyMCEConfiguration.Plugins.GetEnumerator();
                    while (pluginEnum.MoveNext())
                    {
                        var plugin = (tinyMCEPlugin)pluginEnum.Value;
                        if (plugin.UseOnFrontend || (!onFront && !plugin.UseOnFrontend))
                            _plugins += "," + plugin.Name;
                    }

                    // add the umbraco overrides to the end
                    // NB: It is !!REALLY IMPORTANT!! that these plugins are added at the end
                    // as they make runtime modifications to default plugins, so require
                    // those plugins to be loaded first.
                    _plugins += ",umbracopaste,umbracolink,umbracocontextmenu";

                    if (_plugins.StartsWith(","))
                        _plugins = _plugins.Substring(1, _plugins.Length - 1);
                    if (_plugins.EndsWith(","))
                        _plugins = _plugins.Substring(0, _plugins.Length - 1);

                    config.Add("plugins", _plugins);

                    // Check advanced settings
                    if (UmbracoEnsuredPage.CurrentUser != null && ("," + _advancedUsers + ",").IndexOf("," + UmbracoEnsuredPage.CurrentUser.UserType.Id + ",") >
                        -1)
                        config.Add("umbraco_advancedMode", "true");
                    else
                        config.Add("umbraco_advancedMode", "false");

                    // Check maximum image width
                    config.Add("umbraco_maximumDefaultImageWidth", m_maxImageWidth.ToString());

                    // Styles
                    string cssFiles = String.Empty;
                    string styles = string.Empty;
                    foreach (string styleSheetId in _stylesheets)
                    {
                        if (styleSheetId.Trim() != "")
                            try
                            {

                                //TODO: Fix this, it will no longer work!
                                var s = StyleSheet.GetStyleSheet(int.Parse(styleSheetId), false, false);

                                if (s.nodeObjectType == StyleSheet.ModuleObjectType)
                                {
                                    cssFiles += IOHelper.ResolveUrl(SystemDirectories.Css + "/" + s.Text + ".css");

                                    foreach (StylesheetProperty p in s.Properties)
                                    {
                                        if (styles != string.Empty)
                                        {
                                            styles += ";";
                                        }
                                        if (p.Alias.StartsWith("."))
                                            styles += p.Text + "=" + p.Alias;
                                        else
                                            styles += p.Text + "=" + p.Alias;
                                    }

                                    cssFiles += ",";
                                }
                            }
                            catch (Exception ee)
                            {
								LogHelper.Error<TinyMCE>("Error adding stylesheet to tinymce Id:" + styleSheetId, ee);
                            }
                    }
                    // remove any ending comma (,)
                    if (!string.IsNullOrEmpty(cssFiles))
                    {
                        cssFiles = cssFiles.TrimEnd(',');
                    }

                    // language
                    string userLang = (UmbracoEnsuredPage.CurrentUser != null) ?
                        (User.GetCurrent().Language.Contains("-") ?
                            User.GetCurrent().Language.Substring(0, User.GetCurrent().Language.IndexOf("-")) : User.GetCurrent().Language)
                        : "en";
                    config.Add("language", userLang);

                    config.Add("content_css", cssFiles);
                    config.Add("theme_umbraco_styles", styles);

                    // Add buttons
                    IDictionaryEnumerator ide = tinyMCEConfiguration.Commands.GetEnumerator();
                    while (ide.MoveNext())
                    {
                        var cmd = (tinyMCECommand)ide.Value;
                        if (_editorButtons.IndexOf("," + cmd.Alias + ",") > -1)
                            _activateButtons += cmd.Alias + ",";
                        else
                            _disableButtons += cmd.Alias + ",";
                    }

                    if (_activateButtons.Length > 0)
                        _activateButtons = _activateButtons.Substring(0, _activateButtons.Length - 1);
                    if (_disableButtons.Length > 0)
                        _disableButtons = _disableButtons.Substring(0, _disableButtons.Length - 1);

                    // Add buttons
                    initButtons();
                    _activateButtons = "";

                    int separatorPriority = 0;
                    ide = _mceButtons.GetEnumerator();
                    while (ide.MoveNext())
                    {
                        string mceCommand = ide.Value.ToString();
                        var curPriority = (int)ide.Key;

                        // Check priority
                        if (separatorPriority > 0 &&
                            Math.Floor(decimal.Parse(curPriority.ToString()) / 10) >
                            Math.Floor(decimal.Parse(separatorPriority.ToString()) / 10))
                            _activateButtons += "separator,";

                        _activateButtons += mceCommand + ",";

                        separatorPriority = curPriority;
                    }


                    config.Add("theme_umbraco_buttons1", _activateButtons);
                    config.Add("theme_umbraco_buttons2", "");
                    config.Add("theme_umbraco_buttons3", "");
                    config.Add("theme_umbraco_toolbar_align", "left");
                    config.Add("theme_umbraco_disable", _disableButtons);

                    config.Add("theme_umbraco_path ", "true");
                    config.Add("extended_valid_elements", "div[*]");
                    config.Add("document_base_url", "/");
                    config.Add("relative_urls", "false");
                    config.Add("remove_script_host", "true");
                    config.Add("event_elements", "div");
                    config.Add("paste_auto_cleanup_on_paste", "true");

                    config.Add("valid_elements",
                               tinyMCEConfiguration.ValidElements.Substring(1,
                                                                            tinyMCEConfiguration.ValidElements.Length -
                                                                            2));
                    config.Add("invalid_elements", tinyMCEConfiguration.InvalidElements);

                    // custom commands
                    if (tinyMCEConfiguration.ConfigOptions.Count > 0)
                    {
                        ide = tinyMCEConfiguration.ConfigOptions.GetEnumerator();
                        while (ide.MoveNext())
                        {
                            config.Add(ide.Key.ToString(), ide.Value.ToString());
                        }
                    }

                    //if (HttpContext.Current.Request.Path.IndexOf(Umbraco.Core.IO.SystemDirectories.Umbraco) > -1)
                    //    config.Add("language", User.GetUser(BasePage.GetUserId(BasePage.umbracoUserContextID)).Language);
                    //else
                    //    config.Add("language", System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);

                    if (_fullWidth)
                    {
                        config.Add("auto_resize", "true");
                        base.Columns = 30;
                        base.Rows = 30;
                    }
                    else
                    {
                        base.Columns = 0;
                        base.Rows = 0;
                    }
                    EnableViewState = false;
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Incorrect TinyMCE configuration.", "Configuration", ex);
            }
        }

        #region TreatAsRichTextEditor

        public virtual bool TreatAsRichTextEditor
        {
            get { return false; }
        }

        #endregion

        #region ShowLabel

        public virtual bool ShowLabel
        {
            get { return _showLabel; }
        }

        #endregion

        #region Editor

        public Control Editor
        {
            get { return this; }
        }

        #endregion

        #region Save()

        public virtual void Save()
        {
            string parsedString = base.Text.Trim();
            string parsedStringForTinyMce = parsedString;
            if (parsedString != string.Empty)
            {
                parsedString = replaceMacroTags(parsedString).Trim();

                // tidy html - refactored, see #30534
                if (UmbracoConfig.For.UmbracoSettings().Content.TidyEditorContent)
                {
                    // always wrap in a <div> - using <p> was a bad idea
                    parsedString = "<div>" + parsedString + "</div>";

                    string tidyTxt = library.Tidy(parsedString, false);
                    if (tidyTxt != "[error]")
                    {
                        parsedString = tidyTxt;

                        // remove pesky \r\n, and other empty chars
                        parsedString = parsedString.Trim(new char[] { '\r', '\n', '\t', ' ' });

                        // compensate for breaking macro tags by tidy (?)
                        parsedString = parsedString.Replace("/?>", "/>");

                        // remove the wrapping <div> - safer to check that it is still here
                        if (parsedString.StartsWith("<div>") && parsedString.EndsWith("</div>"))
                            parsedString = parsedString.Substring("<div>".Length, parsedString.Length - "<div></div>".Length);
                    }
                }

                // rescue umbraco tags
                parsedString = parsedString.Replace("|||?", "<?").Replace("/|||", "/>").Replace("|*|", "\"");

                // fix images
                parsedString = tinyMCEImageHelper.cleanImages(parsedString);

                // parse current domain and instances of slash before anchor (to fix anchor bug)
                // NH 31-08-2007
                if (HttpContext.Current.Request.ServerVariables != null)
                {
                    parsedString = parsedString.Replace(helper.GetBaseUrl(HttpContext.Current) + "/#", "#");
                    parsedString = parsedString.Replace(helper.GetBaseUrl(HttpContext.Current), "");
                }
	            // if a paragraph is empty, remove it
	            if (parsedString.ToLower() == "<p></p>")
                parsedString = "";
	  
	            // save string after all parsing is done, but before CDATA replacement - to put back into TinyMCE
	            parsedStringForTinyMce = parsedString;

                //Allow CDATA nested into RTE without exceptions
                // GE 2012-01-18
                parsedString = parsedString.Replace("<![CDATA[", "<!--CDATAOPENTAG-->").Replace("]]>", "<!--CDATACLOSETAG-->");
            }

            _data.Value = parsedString;

            // update internal webcontrol value with parsed result
            base.Text = parsedStringForTinyMce;
        }

        #endregion

        public virtual string Plugins
        {
            get { return _plugins; }
            set { _plugins = value; }
        }

        public object[] MenuIcons
        {
            get
            {
                initButtons();

                var tempIcons = new object[_menuIcons.Count];
                for (int i = 0; i < _menuIcons.Count; i++)
                    tempIcons[i] = _menuIcons[i];
                return tempIcons;
            }
        }


        #region IMenuElement Members

        public string ElementName
        {
            get { return "div"; }
        }

        public string ElementIdPreFix
        {
            get { return "umbTinymceMenu"; }
        }

        public string ElementClass
        {
            get { return "tinymceMenuBar"; }
        }

        public int ExtraMenuWidth
        {
            get
            {
                initButtons();
                return _buttons.Count * 40 + 300;
            }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // add current page info
                base.NodeId = ((cms.businesslogic.datatype.DefaultData)_data).NodeId;
                if (NodeId != 0)
                {
                    base.VersionId = ((cms.businesslogic.datatype.DefaultData)_data).Version;
                    config.Add("theme_umbraco_pageId", base.NodeId.ToString());
                    config.Add("theme_umbraco_versionId", base.VersionId.ToString());

                    // we'll need to make an extra check for the liveediting as that value is set after the constructor have initialized
                    config.Add("umbraco_toolbar_id",
                                   ElementIdPreFix +
                                   ((cms.businesslogic.datatype.DefaultData)_data).PropertyId);
                }
                else
                {
                    // this is for use when tinymce is used for non default Umbraco pages
                    config.Add("umbraco_toolbar_id",
                               ElementIdPreFix + "_" + this.ClientID);
                }
            }
            catch
            {
                // Empty catch as this is caused by the document doesn't exists yet,
                // like when using this on an autoform, partly replaced by the if/else check on nodeId above though
            }
            base.OnLoad(e);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            //Allow CDATA nested into RTE without exceptions
            // GE 2012-01-18
            if (_data != null && _data.Value != null)
                base.Text = _data.Value.ToString().Replace("<!--CDATAOPENTAG-->", "<![CDATA[").Replace("<!--CDATACLOSETAG-->", "]]>");
        }

        private string replaceMacroTags(string text)
        {
            while (findStartTag(text) > -1)
            {
                string result = text.Substring(findStartTag(text), findEndTag(text) - findStartTag(text));
                text = text.Replace(result, generateMacroTag(result));
            }
            return text;
        }

        private string generateMacroTag(string macroContent)
        {
            string macroAttr = macroContent.Substring(5, macroContent.IndexOf(">") - 5);
            string macroTag = "|||?UMBRACO_MACRO ";
            Hashtable attributes = ReturnAttributes(macroAttr);
            IDictionaryEnumerator ide = attributes.GetEnumerator();
            while (ide.MoveNext())
            {
                if (ide.Key.ToString().IndexOf("umb_") != -1)
                {
                    // Hack to compensate for Firefox adding all attributes as lowercase
                    string orgKey = ide.Key.ToString();
                    if (orgKey == "umb_macroalias")
                        orgKey = "umb_macroAlias";

                    macroTag += orgKey.Substring(4, orgKey.Length - 4) + "=|*|" +
                                ide.Value.ToString().Replace("\\r\\n", Environment.NewLine) + "|*| ";
                }
            }
            macroTag += "/|||";

            return macroTag;
        }

		[Obsolete("Has been superceded by Umbraco.Core.XmlHelper.GetAttributesFromElement")]
		public static Hashtable ReturnAttributes(String tag)
		{
			var h = new Hashtable();
			foreach (var i in Umbraco.Core.XmlHelper.GetAttributesFromElement(tag))
			{
				h.Add(i.Key, i.Value);
			}
			return h;
		}

        private int findStartTag(string text)
        {
            string temp = "";
            text = text.ToLower();
            if (text.IndexOf("ismacro=\"true\"") > -1)
            {
                temp = text.Substring(0, text.IndexOf("ismacro=\"true\""));
                return temp.LastIndexOf("<");
            }
            return -1;
        }

        private int findEndTag(string text)
        {
            string find = "<!-- endumbmacro -->";

            int endMacroIndex = text.ToLower().IndexOf(find);
            string tempText = text.ToLower().Substring(endMacroIndex + find.Length,
                                                       text.Length - endMacroIndex - find.Length);
            int finalEndPos = 0;
            while (tempText.Length > 5)
                if (tempText.Substring(finalEndPos, 6) == "</div>")
                    break;
                else
                    finalEndPos++;

            return endMacroIndex + find.Length + finalEndPos + 6;
        }

        private void initButtons()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;

                // Add icons for the editor control:
                // Html
                // Preview
                // Style picker, showstyles
                // Bold, italic, Text Gen
                // Align: left, center, right
                // Lists: Bullet, Ordered, indent, undo indent
                // Link, Anchor
                // Insert: Image, macro, table, formular

                foreach (string button in _activateButtons.Split(','))
                {
                    if (button.Trim() != "")
                    {
                        try
                        {
                            var cmd = (tinyMCECommand)tinyMCEConfiguration.Commands[button];

                            string appendValue = "";
                            if (cmd.Value != "")
                                appendValue = ", '" + cmd.Value + "'";
                            _mceButtons.Add(cmd.Priority, cmd.Command);
                            _buttons.Add(cmd.Priority,
                                         new editorButton(cmd.Alias, ui.Text("buttons", cmd.Alias), cmd.Icon,
                                                          "tinyMCE.execInstanceCommand('" + ClientID + "', '" +
                                                          cmd.Command + "', " + cmd.UserInterface + appendValue + ")"));
                        }
                        catch (Exception ee)
                        {
							LogHelper.Error<TinyMCE>(string.Format("TinyMCE: Error initializing button '{0}'", button), ee);
                        }
                    }
                }

                // add save icon
                int separatorPriority = 0;
                IDictionaryEnumerator ide = _buttons.GetEnumerator();
                while (ide.MoveNext())
                {
                    object buttonObj = ide.Value;
                    var curPriority = (int)ide.Key;

                    // Check priority
                    if (separatorPriority > 0 &&
                        Math.Floor(decimal.Parse(curPriority.ToString()) / 10) >
                        Math.Floor(decimal.Parse(separatorPriority.ToString()) / 10))
                        _menuIcons.Add("|");

                    try
                    {
                        var e = (editorButton)buttonObj;

                        MenuIconI menuItem = new MenuIconClass();

                        menuItem.OnClickCommand = e.onClickCommand;
                        menuItem.ImageURL = e.imageUrl;
                        menuItem.AltText = e.alttag;
                        menuItem.ID = e.id;
                        _menuIcons.Add(menuItem);
                    }
                    catch
                    {
                    }

                    separatorPriority = curPriority;
                }
            }
        }
    }
}