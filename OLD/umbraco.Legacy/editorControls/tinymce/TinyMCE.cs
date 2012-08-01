using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.editorControls.wysiwyg;
using umbraco.interfaces;
using umbraco.uicontrols;

namespace umbraco.editorControls.tinymce {
    public class TinyMCE : webcontrol.TinyMCE, IDataEditor, IMenuElement {
        private IData _data;
        private bool _enableContextMenu = false;
        private string _editorButtons = "";
        private string _advancedUsers = "";
        private bool _fullWidth = false;
        private int _width = 0;
        private int _height = 0;
        private bool _showLabel = false;
        private string _plugins = "";
        private ArrayList _stylesheets = new ArrayList();

        private ArrayList _menuIcons = new ArrayList();
        private SortedList _buttons = new SortedList();
        private SortedList _mceButtons = new SortedList();
        private bool _isInitialized = false;
        private string _activateButtons = "";
        private string _disableButtons = "help,visualaid,";
        private int m_maxImageWidth = 500;

        public virtual string Plugins {
            get { return _plugins; }
            set { _plugins = value; }
        }

        public TinyMCE(IData Data, string Configuration) {
            _data = Data;
            string[] configSettings = Configuration.Split("|".ToCharArray());

            if (configSettings.Length > 0) {
                _editorButtons = configSettings[0];

                if (configSettings.Length > 1)
                    if (configSettings[1] == "1")
                        _enableContextMenu = true;

                if (configSettings.Length > 2)
                    _advancedUsers = configSettings[2];

                if (configSettings.Length > 3) {
                    if (configSettings[3] == "1")
                        _fullWidth = true;
                    else if (configSettings[4].Split(',').Length > 1) {
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
                if (configSettings.Length > 4) {
                    foreach (string s in configSettings[5].Split(",".ToCharArray()))
                        _stylesheets.Add(s);
                }

                if (configSettings.Length > 6 && configSettings[6] != "")
                    _showLabel = bool.Parse(configSettings[6]);

                if (configSettings.Length > 7 && configSettings[7] != "")
                    m_maxImageWidth = int.Parse(configSettings[7].ToString());

                // sizing
                if (!_fullWidth) {
                    config.Add("width", _width.ToString());
                    config.Add("height", _height.ToString());
                }

                if (_enableContextMenu)
                    _plugins += ",contextmenu";

                //                config.Add("theme_advanced_statusbar_location", "none");

                // If the editor is used in umbraco, use umbraco's own toolbar
                bool onFront = false;
                if (HttpContext.Current.Request.Path.ToLower().IndexOf(GlobalSettings.Path.ToLower()) > -1)
                    config.Add("theme_advanced_toolbar_location", "external");
                else {
                    onFront = true;
                    config.Add("theme_advanced_toolbar_location", "top");
                }

                // load plugins
                IDictionaryEnumerator pluginEnum = tinyMCEConfiguration.Plugins.GetEnumerator();
                while (pluginEnum.MoveNext()) {
                    tinyMCEPlugin plugin = (tinyMCEPlugin)pluginEnum.Value;
                    if (plugin.UseOnFrontend || (!onFront && !plugin.UseOnFrontend))
                        _plugins += "," + plugin.Name;
                }
                if (_plugins.StartsWith(","))
                    _plugins = _plugins.Substring(1, _plugins.Length - 1);
                if (_plugins.EndsWith(","))
                    _plugins = _plugins.Substring(0, _plugins.Length - 1);

                config.Add("plugins", _plugins);

                // Check advanced settings
                if (("," + _advancedUsers + ",").IndexOf("," + UmbracoEnsuredPage.CurrentUser.UserType.Id + ",") > -1)
                    config.Add("umbracoAdvancedMode", "true");
                else
                    config.Add("umbracoAdvancedMode", "false");

                // Check maximum image width
                config.Add("umbracoMaximumDefaultImageWidth", m_maxImageWidth.ToString());

                // Styles
                string cssFiles = String.Empty;
                string styles = string.Empty;
                foreach (string styleSheetId in _stylesheets) {
                    if (styleSheetId.Trim() != "")
                        try {
                            StyleSheet s = StyleSheet.GetStyleSheet(int.Parse(styleSheetId), false, false);
                            cssFiles += GlobalSettings.Path + "/../css/" + s.Text + ".css";

                            foreach (StylesheetProperty p in s.Properties) {
                                if (styles != string.Empty) {
                                    styles += ";";
                                }
                                if (p.Alias.StartsWith("."))
                                    styles += p.Text + "=" + p.Alias;
                                else
                                    styles += p.Text + "=" + p.Alias;
                            }

                            cssFiles += ",";
                        } catch (Exception ee) {
                            Log.Add(LogTypes.Error, -1,
                                    string.Format(
                                        string.Format("Error adding stylesheet to tinymce (id: {{0}}). {0}", ee),
                                        styleSheetId));
                        }
                }

                config.Add("content_css", cssFiles);
                config.Add("theme_advanced_styles", styles);

                // Add buttons
                IDictionaryEnumerator ide = tinyMCEConfiguration.Commands.GetEnumerator();
                while (ide.MoveNext()) {
                    tinyMCECommand cmd = (tinyMCECommand)ide.Value;
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
                while (ide.MoveNext()) {
                    string mceCommand = ide.Value.ToString();
                    int curPriority = (int)ide.Key;

                    // Check priority
                    if (separatorPriority > 0 &&
                        Math.Floor(decimal.Parse(curPriority.ToString()) / 10) >
                        Math.Floor(decimal.Parse(separatorPriority.ToString()) / 10))
                        _activateButtons += "separator,";

                    _activateButtons += mceCommand + ",";

                    separatorPriority = curPriority;
                }


                config.Add("theme_advanced_buttons1", _activateButtons);
                config.Add("theme_advanced_buttons2", "");
                config.Add("theme_advanced_buttons3", "");
                config.Add("theme_advanced_toolbar_align", "left");
                config.Add("theme_advanced_disable", _disableButtons);

                config.Add("theme_advanced_path ", "true");
                config.Add("extended_valid_elements", "div[*]");
                config.Add("document_base_url", "/");
                config.Add("event_elements", "div");


                if (HttpContext.Current.Request.Path.IndexOf(GlobalSettings.Path) > -1)
                    config.Add("language", User.GetUser(BasePage.GetUserId(BasePage.umbracoUserContextID)).Language);
                else
                    config.Add("language", System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);

                if (_fullWidth) {
                    config.Add("auto_resize", "true");
                    base.Cols = 30;
                    base.Rows = 30;
                } else {
                    base.Cols = 0;
                    base.Rows = 0;
                }
            }
        }

        #region IDataEditor Members

        #region TreatAsRichTextEditor

        public virtual bool TreatAsRichTextEditor {
            get { return false; }
        }

        #endregion

        #region ShowLabel

        public virtual bool ShowLabel {
            get { return _showLabel; }
        }

        #endregion

        #region Editor

        public Control Editor {
            get { return this; }
        }

        #endregion

        #region Save()

        public virtual void Save() {
            string parsedString = base.Text.Trim();
            if (parsedString != string.Empty) {
                parsedString = replaceMacroTags(parsedString).Trim();

                // clean macros and add paragraph element for tidy
                bool removeParagraphs = false;
                if (parsedString.Length > 16 && parsedString.ToLower().Substring(0, 17) == "|||?umbraco_macro") {
                    removeParagraphs = true;
                    parsedString = "<p>" + parsedString + "</p>";
                }

                // tidy html

                if (UmbracoSettings.TidyEditorContent) {
                    string tidyTxt = library.Tidy(parsedString, false);
                    if (tidyTxt != "[error]") {
                        // compensate for breaking macro tags by tidy
                        parsedString = tidyTxt.Replace("/?>", "/>");
                        if (removeParagraphs) {
                            if (parsedString.Length - parsedString.Replace("<", "").Length == 2)
                                parsedString = parsedString.Replace("<p>", "").Replace("</p>", "");
                        }
                    } else {
                        // TODO
                        // How to log errors? _data.NodeId does not exist?
                        //BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, BusinessLogic.User.GetUser(0), _data.NodeId, "Error tidying txt from property: " + _data.PropertyId.ToString());
                    }
                }

                // rescue umbraco tags
                parsedString = parsedString.Replace("|||?", "<?").Replace("/|||", "/>").Replace("|*|", "\"");

                // fix images
                parsedString = tinyMCEImageHelper.cleanImages(parsedString);

                // parse current domain and instances of slash before anchor (to fix anchor bug)
                // NH 31-08-2007
                if (HttpContext.Current.Request.ServerVariables != null) {
                    parsedString = parsedString.Replace(helper.GetBaseUrl(HttpContext.Current) + "/#", "#");
                    parsedString = parsedString.Replace(helper.GetBaseUrl(HttpContext.Current), "");
                }

                // if text is an empty parargraph, make it all empty
                if (parsedString.ToLower() == "<p></p>")
                    parsedString = "";
            }
            _data.Value = parsedString;

            // update internal webcontrol value with parsed result
            base.Text = parsedString;
        }

        #endregion

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            try {
                base.NodeId = ((cms.businesslogic.datatype.DefaultData)_data).NodeId;
                base.VersionId = ((cms.businesslogic.datatype.DefaultData)_data).Version;
                config.Add("umbraco_toolbar_id",
                           ElementIdPreFix + ((cms.businesslogic.datatype.DefaultData)_data).PropertyId.ToString());
            } catch {
                // Empty catch as this is caused by the document doesn't exists yet,
                // like when using this on an autoform
            }
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "tinymce",
                                                        "<script language=\"javascript\" type=\"text/javascript\" src=\"" +
                                                        JavascriptLocation + CoreFile + "\"></script>");
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "tinymceUmbPasteCheck",
                                                        "<script language=\"javascript\" type=\"text/javascript\" src=\"" +
                                                        JavascriptLocation +
                                                        "/plugins/umbracoAdditions/umbPasteCheck.js\"></script>");
        }

        protected override void Render(HtmlTextWriter output) {
            base.JavascriptLocation = GlobalSettings.ClientPath + "/tinymce/tiny_mce.js";
            base.Render(output);
        }

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);
            if (!Page.IsPostBack) {
                if (_data != null && _data.Value != null)
                    base.Text = _data.Value.ToString();
            }
        }

        private string replaceMacroTags(string text) {
            while (findStartTag(text) > -1) {
                string result = text.Substring(findStartTag(text), findEndTag(text) - findStartTag(text));
                text = text.Replace(result, generateMacroTag(result));
            }
            return text;
        }

        private string generateMacroTag(string macroContent) {
            string macroAttr = macroContent.Substring(5, macroContent.IndexOf(">") - 5);
            string macroTag = "|||?UMBRACO_MACRO ";
            Hashtable attributes = ReturnAttributes(macroAttr);
            IDictionaryEnumerator ide = attributes.GetEnumerator();
            while (ide.MoveNext()) {
                if (ide.Key.ToString().IndexOf("umb_") != -1) {
                    // Hack to compensate for Firefox adding all attributes as lowercase
                    string orgKey = ide.Key.ToString();
                    if (orgKey == "umb_macroalias")
                        orgKey = "umb_macroAlias";

                    macroTag += orgKey.Substring(4, orgKey.ToString().Length - 4) + "=|*|" +
                                ide.Value.ToString() + "|*| ";
                }
            }
            macroTag += "/|||";

            return macroTag;
        }

        public static Hashtable ReturnAttributes(String tag) {
            Hashtable ht = new Hashtable();
            MatchCollection m =
                Regex.Matches(tag, "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"",
                              RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            foreach (Match attributeSet in m)
                ht.Add(attributeSet.Groups["attributeName"].Value.ToString(),
                       attributeSet.Groups["attributeValue"].Value.ToString());

            return ht;
        }

        private int findStartTag(string text) {
            string temp = "";
            text = text.ToLower();
            if (text.IndexOf("ismacro=\"true\"") > -1) {
                temp = text.Substring(0, text.IndexOf("ismacro=\"true\""));
                return temp.LastIndexOf("<");
            }
            return -1;
        }

        private int findEndTag(string text) {
            string find = "<!-- endumbmacro -->";

            int endMacroIndex = text.ToLower().IndexOf(find);
            string tempText = text.ToLower().Substring(endMacroIndex + find.Length, text.Length - endMacroIndex - find.Length);
            int finalEndPos = 0;
            while (tempText.Length > 5)
                if (tempText.Substring(finalEndPos, 6) == "</div>")
                    break;
                else
                    finalEndPos++;

            return endMacroIndex + find.Length + finalEndPos + 6;
        }

        #endregion

        #region IDataFieldWithButtons Members

        public object[] MenuIcons {
            get {
                initButtons();

                object[] tempIcons = new object[_menuIcons.Count];
                for (int i = 0; i < _menuIcons.Count; i++)
                    tempIcons[i] = _menuIcons[i];
                return tempIcons;
            }
        }

        private void initButtons() {
            if (!_isInitialized) {
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

                foreach (string button in _activateButtons.Split(',')) {
                    if (button.Trim() != "") {
                        try {
                            tinyMCECommand cmd = (tinyMCECommand)tinyMCEConfiguration.Commands[button];

                            string appendValue = "";
                            if (cmd.Value != "")
                                appendValue = ", '" + cmd.Value + "'";
                            _mceButtons.Add(cmd.Priority, cmd.Command);
                            _buttons.Add(cmd.Priority,
                                         new editorButton(cmd.Alias, ui.Text("buttons", cmd.Alias, null), cmd.Icon,
                                                          "tinyMCE.execInstanceCommand('" + ClientID + "', '" +
                                                          cmd.Command + "', " + cmd.UserInterface + appendValue + ")"));
                        } catch (Exception ee) {
                            Log.Add(LogTypes.Error, User.GetUser(0), -1,
                                    string.Format("TinyMCE: Error initializing button '{0}': {1}", button, ee.ToString()));
                        }
                    }
                }

                // add save icon
                int separatorPriority = 0;
                IDictionaryEnumerator ide = _buttons.GetEnumerator();
                while (ide.MoveNext()) {
                    object buttonObj = ide.Value;
                    int curPriority = (int)ide.Key;

                    // Check priority
                    if (separatorPriority > 0 &&
                        Math.Floor(decimal.Parse(curPriority.ToString()) / 10) >
                        Math.Floor(decimal.Parse(separatorPriority.ToString()) / 10))
                        _menuIcons.Add("|");

                    try {
                        editorButton e = (editorButton)buttonObj;

                        MenuIconI menuItem = new MenuIconClass();

                        menuItem.OnClickCommand = e.onClickCommand;
                        menuItem.ImageURL = e.imageUrl;
                        menuItem.AltText = e.alttag;
                        menuItem.ID = e.id;
                        _menuIcons.Add(menuItem);
                    } catch {
                    }

                    separatorPriority = curPriority;
                }
            }
        }

        #endregion

        #region IMenuElement Members

        public string ElementName {
            get { return "div"; }
        }

        public string ElementIdPreFix {
            get { return "umbTinymceMenu"; }
        }

        public string ElementClass {
            get { return "tinymceMenuBar"; }
        }

        public int ExtraMenuWidth {
            get {
                initButtons();
                return _buttons.Count * 40 + 300;
            }
        }

        #endregion
    }
}