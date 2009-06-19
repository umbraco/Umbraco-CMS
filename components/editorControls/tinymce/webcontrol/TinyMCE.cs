using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.property;
using Content = umbraco.cms.businesslogic.Content;

namespace umbraco.editorControls.tinymce.webcontrol {
    public class TinyMCE : WebControl, IPostBackDataHandler {
        private string _javascriptLocation = "/umbraco_client/tinymce";
        private string _coreFile = "/tiny_mce.js";
        private int _rows = 13;
        private int _cols = 60;
        public NameValueCollection config;
        private string temp;

        private int _nodeId = -1;
        private Guid _versionId;

        public int NodeId {
            set { _nodeId = value; }
        }

        public Guid VersionId {
            set { _versionId = value; }
        }

        public string CoreFile {
            get { return _coreFile; }
        }


        public TinyMCE() {
            config = new NameValueCollection();
            config.Add("mode", "exact");
            config.Add("theme", "advanced");

            //content_css : "/mycontent.css"
            temp = string.Empty;


            //this._config.SetStringParam(TinyMCEConfig.StringParameter.mode, "exact");
            //this._config.SetStringParam(TinyMCEConfig.StringParameter.theme, "advanced");
            //this._config.SetStringParam(TinyMCEConfig.StringParameter.plugins, "advlink,noneditable,advimage,flash");
            //this._config.SetStringParam(TinyMCEConfig.StringParameter.theme_advanced_buttons3_add, "flash");
        }

        // <summary>
        /// The text of the editor
        /// </summary>
        public string Text {
            get { return (string)ViewState["text"]; }
            set { ViewState["text"] = value; }
        }

        // <summary>
        /// The number of rows in the textarea that gets converted to the editor.
        /// This affects the size of the editor. Default is 10
        /// </summary>
        public int Rows {
            get { return _rows; }
            set { _rows = value; }
        }

        // <summary>
        /// The number of columns in the textarea that gets converted to the editor.
        /// This affects the size of the editor. Default is 40.
        /// </summary>
        public int Cols {
            get { return _cols; }
            set { _cols = value; }
        }

        // <summary>
        /// Path to the TinyMCE javascript, default is "jscripts/tiny_mce/tiny_mce.js"
        /// </summary>
        public string JavascriptLocation {
            get { return _javascriptLocation; }
            set { _javascriptLocation = value; }
        }

        /// <summary>
        /// Draws the editor
        /// </summary>
        /// <param name="outWriter">The writer to draw the editor to</param>
        protected override void Render(HtmlTextWriter writer) {
            writer.WriteLine("<!-- tinyMCE -->\n");
            //            writer.WriteLine("<script language=\"javascript\" type=\"text/javascript\" src=\"" + JavascriptLocation + "\"/>\n");
            writer.WriteLine("<script language=\"javascript\" type=\"text/javascript\">");
            writer.WriteLine("tinyMCE.init({");

            for (int i = 0; i < config.Count; i++) {
                writer.WriteLine(config.GetKey(i) + " : \"" + config.Get(i) + "\",");
            }
            writer.WriteLine("theme_advanced_resizing : true,");
            writer.WriteLine("theme_advanced_resize_horizontal : false,");
            writer.WriteLine("apply_source_formatting : true,");
            writer.WriteLine("relative_urls : false,");
            writer.WriteLine("valid_elements : {0},", tinyMCEConfiguration.ValidElements);
            writer.WriteLine("invalid_elements : \"{0}\",", tinyMCEConfiguration.InvalidElements);

            writer.WriteLine("elements : \"" + UniqueID + "\",");
            writer.WriteLine("umbracoPath: \"" + GlobalSettings.Path + "\"");


            writer.WriteLine("});");

            // write a message that this is deprecated
            writer.WriteLine("alert('This page use an old version of the Richtext Editor (TinyMCE2), which is deprecated and no longer supported in Umbraco 4. \\n\\nPlease upgrade by going to\\n- Developer\\n- Data Types\\n- Richtext editor\\n- And choose \\'TinyMCE3 wysiwyg\\' as the rendercontrol\\n\\nIf you don\\'t have administrative priviledges in umbraco, you should contact your administrator');");

            writer.WriteLine("</script>\n");
            writer.WriteLine("<!-- /tinyMCE -->\n");

            if (Cols > 0)
                writer.Write("<textarea class=\"tinymce" + UniqueID + "\" id=\"" + UniqueID + "\" name=\"" + UniqueID + "\" cols=\"" + Cols + "\" rows=\"" +
                             Rows + "\" style=\"width: 100%\">\n");
            else
                writer.Write("<textarea class=\"tinymce" + UniqueID + "\" id=\"" + UniqueID + "\" name=\"" + UniqueID + "\">\n");


            // if the document exists, parse it for macros
            if (_nodeId != -1)
                writer.Write(parseMacrosToHtml(formatMedia(Text)));
            else
                writer.Write(Text);
            writer.WriteLine("</textarea>");
        }

        private string formatMedia(string html) {
            // Local media path
            string localMediaPath = getLocalMediaPath();

            // Find all media images
            string pattern = "<img [^>]*src=\"(?<mediaString>/media[^\"]*)\" [^>]*>";

            MatchCollection tags =
                Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            foreach (Match tag in tags)
                if (tag.Groups.Count > 0) {
                    // Replace /> to ensure we're in old-school html mode
                    string tempTag = "<img";
                    string orgSrc = tag.Groups["mediaString"].Value;

                    // gather all attributes
                    // TODO: This should be replaced with a general helper method - but for now we'll wanna leave umbraco.dll alone for this patch
                    Hashtable ht = new Hashtable();
                    MatchCollection m =
                        Regex.Matches(tag.Value.Replace(">", " >"),
                                      "(?<attributeName>\\S*)=\"(?<attributeValue>[^\"]*)\"|(?<attributeName>\\S*)=(?<attributeValue>[^\"|\\s]*)\\s",
                                      RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                    foreach (Match attributeSet in m) {
                        if (attributeSet.Groups["attributeName"].Value.ToString().ToLower() != "src")
                            ht.Add(attributeSet.Groups["attributeName"].Value.ToString(),
                                   attributeSet.Groups["attributeValue"].Value.ToString());
                    }

                    // build the element
                    // Build image tag
                    IDictionaryEnumerator ide = ht.GetEnumerator();
                    while (ide.MoveNext())
                        tempTag += " " + ide.Key.ToString() + "=\"" + ide.Value.ToString() + "\"";

                    // Find the original filename, by removing the might added width and height
                    orgSrc =
                        orgSrc.Replace(
                            "_" + helper.FindAttribute(ht, "width") + "x" + helper.FindAttribute(ht, "height"), "").
                            Replace("%20", " ");

                    // Check for either id or guid from media
                    string mediaId = getIdFromSource(orgSrc, localMediaPath);

                    Media imageMedia = null;

                    try {
                        int mId = int.Parse(mediaId);
                        Property p = new Property(mId);
                        imageMedia = new Media(Content.GetContentFromVersion(p.VersionId).Id);
                    } catch {
                        try {
                            imageMedia = new Media(Content.GetContentFromVersion(new Guid(mediaId)).Id);
                        } catch {
                        }
                    }

                    // Check with the database if any media matches this url
                    if (imageMedia != null) {
                        try {
                            // Check extention
                            if (imageMedia.getProperty("umbracoExtension").Value.ToString() != orgSrc.Substring(orgSrc.LastIndexOf(".") + 1, orgSrc.Length - orgSrc.LastIndexOf(".") - 1))
                                orgSrc = orgSrc.Substring(0, orgSrc.LastIndexOf(".") + 1) +
                                         imageMedia.getProperty("umbracoExtension").Value.ToString();

                            // Format the tag
                            tempTag = tempTag + " rel=\"" +
                                      imageMedia.getProperty("umbracoWidth").Value.ToString() + "," +
                                      imageMedia.getProperty("umbracoHeight").Value.ToString() + "\" src=\"" + orgSrc +
                                      "\"";
                            tempTag += "/>";

                            // Replace the tag
                            html = html.Replace(tag.Value, tempTag);
                        } catch (Exception ee) {
                            Log.Add(LogTypes.Error, User.GetUser(0), -1,
                                    "Error reading size data from media: " + imageMedia.Id.ToString() + ", " +
                                    ee.ToString());
                        }
                    } else
                        Log.Add(LogTypes.Error, User.GetUser(0), -1,
                                "Error reading size data from media (not found): " + orgSrc);
                }
            return html;
        }

        private string getIdFromSource(string src, string localMediaPath) {
            // important - remove out the umbraco path + media!
            src = src.Replace(localMediaPath, "");

            string _id = "";

            // Check for directory id naming 
            if (src.Length - src.Replace("/", "").Length > 0) {
                string[] dirSplit = src.Split("/".ToCharArray());
                string tempId = dirSplit[0];
                try {
                    // id
                    _id = int.Parse(tempId).ToString();
                } catch {
                    // guid
                    _id = tempId;
                }
            } else {
                string[] fileSplit = src.Replace("/media/", "").Split("-".ToCharArray());

                // guid or id
                if (fileSplit.Length > 3) {
                    for (int i = 0; i < 5; i++)
                        _id += fileSplit[i] + "-";
                    _id = _id.Substring(0, _id.Length - 1);
                } else
                    _id = fileSplit[0];
            }

            return _id;
        }

        private string getLocalMediaPath() {
            string[] umbracoPathSplit = GlobalSettings.Path.Split("/".ToCharArray());
            string umbracoPath = "";
            for (int i = 0; i < umbracoPathSplit.Length - 1; i++)
                umbracoPath += umbracoPathSplit[i] + "/";
            return umbracoPath + "media/";
        }


        private string parseMacrosToHtml(string input) {
            int nodeId = _nodeId;
            Guid versionId = _versionId;
            string content = input;


            string pattern = @"(<\?UMBRACO_MACRO\W*[^>]*/>)";
            MatchCollection tags =
                Regex.Matches(content, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            // Page for macro rendering
            page p = new page(nodeId, versionId);
            HttpContext.Current.Items["macrosAdded"] = 0;
            HttpContext.Current.Items["pageID"] = nodeId.ToString();

            foreach (Match tag in tags) {
                try {
                    // Create div
                    Hashtable attributes = helper.ReturnAttributes(tag.Groups[1].Value);
                    string div = macro.renderMacroStartTag(attributes, nodeId, versionId).Replace("&quot;", "&amp;quot;");

                    // Insert macro contents here...
                    macro m;
                    if (helper.FindAttribute(attributes, "macroID") != "")
                        m = new macro(int.Parse(helper.FindAttribute(attributes, "macroID")));
                    else {
                        // legacy: Check if the macroAlias is typed in lowercasing
                        string macroAlias = helper.FindAttribute(attributes, "macroAlias");
                        if (macroAlias == "") {
                            macroAlias = helper.FindAttribute(attributes, "macroalias");
                            attributes.Remove("macroalias");
                            attributes.Add("macroAlias", macroAlias);
                        }

                        if (macroAlias != "")
                            m = new macro(Macro.GetByAlias(macroAlias).Id);
                        else
                            throw new ArgumentException("umbraco is unable to identify the macro. No id or macroalias was provided for the macro in the macro tag.", tag.Groups[1].Value);
                    }

                    if (helper.FindAttribute(attributes, "macroAlias") == "")
                        attributes.Add("macroAlias", m.Alias);


                    try {
                        div += macro.MacroContentByHttp(nodeId, versionId, attributes);
                    } catch {
                        div += "<span style=\"color: green\">No macro content available for WYSIWYG editing</span>";
                    }


                    div += macro.renderMacroEndTag();
                    content = content.Replace(tag.Groups[1].Value, div);
                } catch (Exception ee) {
                    Log.Add(LogTypes.Error, this._nodeId, "Macro Parsing Error: " + ee.ToString());
                    string div = "<div class=\"umbMacroHolder mceNonEditable\"><p style=\"color: red\"><strong>umbraco was unable to parse a macro tag, which means that parts of this content might be corrupt.</strong> <br /><br />Best solution is to rollback to a previous version by right clicking the node in the tree and then try to insert the macro again. <br/><br/>Please report this to your system administrator as well - this error has been logged.</p></div>";
                    content = content.Replace(tag.Groups[1].Value, div);
                }

            }
            return content;
        }

        private static readonly object TextChangedEvent = new object();

        /// <summary>
        /// Raises an event when the text in the editor changes.
        /// </summary>
        public event EventHandler TextChanged {
            add { Events.AddHandler(TextChangedEvent, value); }
            remove { Events.RemoveHandler(TextChangedEvent, value); }
        }

        /// <summary>
        /// Event for text change.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTextChanged(EventArgs e) {
            if (Events != null) {
                EventHandler oEventHandler = (EventHandler)Events[TextChangedEvent];
                if (oEventHandler != null) {
                    oEventHandler(this, e);
                }
            }
        }

        /// <summary>
        /// Called when a postback occurs on the page the control is placed at
        /// </summary>
        /// <param name="postDataKey">The key of the editor data</param>
        /// <param name="postCollection">All the posted data</param>
        /// <returns></returns>
        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            string newText = postCollection[postDataKey];

            if (newText != Text) {
                Text = newText;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises an event when postback occurs
        /// </summary>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            OnTextChanged(EventArgs.Empty);
        }
    }

    public enum TinyMceButtons {
        code,
        bold,
        italic,
        underline,
        strikethrough,
        justifyleft,
        justifycenter,
        justifyright,
        justifyfull,
        bullist,
        numlist,
        outdent,
        indent,
        cut,
        copy,
        pasteword,
        undo,
        redo,
        link,
        unlink,
        image,
        table,
        hr,
        removeformat,
        sub,
        sup,
        charmap,
        anchor,
        umbracomacro
    }
}