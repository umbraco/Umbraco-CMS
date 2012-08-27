using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.property;
using Content = umbraco.cms.businesslogic.Content;
using ClientDependency.Core.Controls;
using ClientDependency.Core;
using umbraco.IO;

namespace umbraco.editorControls.tinyMCE3.webcontrol
{
    public class TinyMCEWebControl : System.Web.UI.WebControls.TextBox
    {
        internal readonly IMediaFileSystem _fs;

        public NameValueCollection config = new NameValueCollection();
        private string temp;

        private int _nodeId = 0;
        private Guid _versionId;

        public bool IsInLiveEditingMode { get; set; }

        public int NodeId
        {
            set { _nodeId = value; }
            get { return _nodeId; }
        }

        public Guid VersionId
        {
            set { _versionId = value; }
            get { return _versionId; }
        }

        public string InstallPath
        {
            get
            {
                if (installPath == null)
                    installPath = this.config["InstallPath"];

                return installPath;
            }
            set { installPath = value; }
        }

        #region private
        private static readonly object TextChangedEvent = new object();
        private int rows = 10;
        private int cols = 70;
        private bool gzipEnabled, merged;
        private string installPath, mode;
        private System.Text.StringBuilder m_scriptInitBlock = new StringBuilder();
        #endregion

        public TinyMCEWebControl()
            : base()
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider<IMediaFileSystem>();

            base.TextMode = TextBoxMode.MultiLine;
            base.Attributes.Add("style", "visibility: hidden");
            config.Add("mode", "exact");
            config.Add("theme", "umbraco");
			config.Add("umbraco_path", Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco));
            CssClass = "tinymceContainer";
            plugin.ConfigSection configSection = (plugin.ConfigSection)System.Web.HttpContext.Current.GetSection("TinyMCE");

            if (configSection != null)
            {
                this.installPath = configSection.InstallPath;
                this.mode = configSection.Mode;
                this.gzipEnabled = configSection.GzipEnabled;

                // Copy items into local config collection
                foreach (string key in configSection.GlobalSettings.Keys)
                    this.config[key] = configSection.GlobalSettings[key];
            }
            else
            {
                configSection = new plugin.ConfigSection();
                configSection.GzipExpiresOffset = TimeSpan.FromDays(10).Ticks;
                this.gzipEnabled = false;
                this.InstallPath = umbraco.editorControls.tinymce.tinyMCEConfiguration.JavascriptPath;

            }

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            // update macro's and images in text
            if (_nodeId != 0)
            {
                base.Text = parseMacrosToHtml(formatMedia(base.Text));
            }

        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            if (!IsInLiveEditingMode)
                writer.Write(m_scriptInitBlock.ToString());
            else
                // add a marker to tell Live Editing when a tinyMCE control is on the page
                writer.Write("<input type='hidden' id='__umbraco_tinyMCE' />");
        }

        protected override void OnLoad(EventArgs args)
        {
            if (!IsInLiveEditingMode)
                this.config["elements"] = this.ClientID;

            bool first = true;

            // Render HTML for TinyMCE instance
            // in the liveediting mode we're always preloading tinymce script
            if (!IsInLiveEditingMode)
            {
                //TinyMCE uses it's own compressor so leave it up to ScriptManager to render
                ScriptManager.RegisterClientScriptInclude(this, this.GetType(), _versionId.ToString(), this.ScriptURI);
            }
            else
            {
                //We're in live edit mode so add the base js file to the dependency list
                ClientDependencyLoader.Instance.RegisterDependency("tinymce3/tiny_mce_src.js",
                    "UmbracoClient", ClientDependencyType.Javascript);
            }

            // Write script tag start
            m_scriptInitBlock.Append(HtmlTextWriter.TagLeftChar.ToString());
            m_scriptInitBlock.Append("script");
            m_scriptInitBlock.Append(" type=\"text/javascript\"");
            m_scriptInitBlock.Append(HtmlTextWriter.TagRightChar.ToString());
            m_scriptInitBlock.Append("\n");

            m_scriptInitBlock.Append("tinyMCE.init({\n");

            // Write options
            foreach (string key in this.config.Keys)
            {
                //TODO: This is a hack to test if we can prevent tinymce from automatically download languages
                if (!IsInLiveEditingMode || (key != "language"))
                {
                    string val = this.config[key];

                    if (!first)
                        m_scriptInitBlock.Append(",\n");
                    else
                        first = false;

                    // Is boolean state or string
                    if (val == "true" || val == "false")
                        m_scriptInitBlock.Append(key + ":" + this.config[key]);
                    else
                        m_scriptInitBlock.Append(key + ":'" + this.config[key] + "'");
                }
            }

            m_scriptInitBlock.Append("\n});\n");
            // we're wrapping the tinymce init call in a load function when in live editing,
            // so we'll need to close that function declaration
            if (IsInLiveEditingMode)
            {
                m_scriptInitBlock.Append(@"(function() { var f =
                    function() {
                        if(document.getElementById('__umbraco_tinyMCE'))
                            tinyMCE.execCommand('mceAddControl',false,'").Append(ClientID).Append(@"'); 
                        ItemEditing.remove_startEdit(f);
                    }
                    ItemEditing.add_startEdit(f);})();");
                m_scriptInitBlock.Append(@"(function() { var f =
                    function() {
                        tinyMCE.execCommand('mceRemoveControl',false,'").Append(ClientID).Append(@"');
                        ItemEditing.remove_stopEdit(f);
                    }
                    ItemEditing.add_stopEdit(f);})();");
            }

            // Write script tag end
            m_scriptInitBlock.Append(HtmlTextWriter.EndTagLeftChars);
            m_scriptInitBlock.Append("script");
            m_scriptInitBlock.Append(HtmlTextWriter.TagRightChar.ToString());

            // add to script manager
            if (IsInLiveEditingMode)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), new Guid().ToString(),
                                            m_scriptInitBlock.ToString(), false);
            }

        }


        string ScriptURI
        {
            get
            {
                string suffix = "", outURI;

                if (this.InstallPath == null)
                    throw new Exception("Required installPath setting is missing, add it to your web.config. You can also add it directly to your tinymce:TextArea element using InstallPath but the web.config method is recommended since it allows you to switch over to gzip compression.");

                if (this.mode != null)
                    suffix = "_" + this.mode;

                outURI = this.InstallPath + "/tiny_mce_src" + suffix + ".js";
				if (!File.Exists(Umbraco.Core.IO.IOHelper.MapPath(outURI)))
					throw new Exception("Could not locate TinyMCE by URI:" + outURI + ", Physical path:" + Umbraco.Core.IO.IOHelper.MapPath(outURI) + ". Make sure that you configured the installPath to a valid location in your web.config. This path should be an relative or site absolute URI to where TinyMCE is located.");

                // Collect themes, languages and plugins and build gzip URI
                // TODO: Make sure gzip is re-enabled
                this.gzipEnabled = true;
                if (this.gzipEnabled)
                {
                    ArrayList themes = new ArrayList(), plugins = new ArrayList(), languages = new ArrayList();

                    // Add themes
                    if (config["theme"] == null)
                        config["theme"] = "simple";

                    foreach (string theme in config["theme"].Split(','))
                    {
                        string themeVal = theme.Trim();

                        if (themes.IndexOf(themeVal) == -1)
                            themes.Add(themeVal);
                    }

                    // Add plugins
                    if (config["plugins"] != null)
                    {
                        foreach (string plugin in config["plugins"].Split(','))
                        {
                            string pluginVal = plugin.Trim();

                            if (plugins.IndexOf(pluginVal) == -1)
                                plugins.Add(pluginVal);
                        }
                    }

                    // Add language
                    if (config["language"] == null)
                        config["language"] = "en";

                    if (languages.IndexOf(config["language"]) == -1)
                        languages.Add(config["language"]);

                    // NH: No clue why these extra dashes are added, but the affect other parts of the implementation
                    // Skip loading of themes and plugins
                    //                        area.config["theme"] = "-" + area.config["theme"];

                    //                        if (area.config["plugins"] != null)
                    //                            area.config["plugins"] = "-" + String.Join(",-", area.config["plugins"].Split(','));

                    // Build output URI
                    // NH: Use versionId for randomize to ensure we don't download a new tinymce on every single call
                    outURI = umbraco.editorControls.tinymce.tinyMCEConfiguration.PluginPath + "/tinymce3tinymceCompress.aspx?rnd=" + this._versionId.ToString() + "&module=gzipmodule";

                    if (themes.Count > 0)
                        outURI += "&themes=" + String.Join(",", ((string[])themes.ToArray(typeof(string))));

                    if (plugins.Count > 0)
                        outURI += "&plugins=" + String.Join(",", ((string[])plugins.ToArray(typeof(string))));

                    if (languages.Count > 0)
                        outURI += "&languages=" + String.Join(",", ((string[])languages.ToArray(typeof(string))));
                }

                return outURI;
            }
        }


        private string formatMedia(string html)
        {
            // root media url
            var rootMediaUrl = _fs.GetUrl("");

            // Find all media images
            var pattern = String.Format("<img [^>]*src=\"(?<mediaString>{0}[^\"]*)\" [^>]*>", rootMediaUrl);

            MatchCollection tags =
                Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            
            foreach (Match tag in tags)
                if (tag.Groups.Count > 0)
                {
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

                    //GE: Add ContainsKey check and expand the ifs for readability
                    foreach (Match attributeSet in m)
                    {
                        if (attributeSet.Groups["attributeName"].Value.ToString().ToLower() != "src")
                        {
                            if (!ht.ContainsKey(attributeSet.Groups["attributeName"].Value.ToString()))
                            {
                                ht.Add(attributeSet.Groups["attributeName"].Value.ToString(), attributeSet.Groups["attributeValue"].Value.ToString());
                            }
                        }
                    }

                    // build the element
                    // Build image tag
                    IDictionaryEnumerator ide = ht.GetEnumerator();
                    while (ide.MoveNext())
                        tempTag += " " + ide.Key.ToString() + "=\"" + ide.Value.ToString() + "\"";

                    // Find the original filename, by removing the might added width and height
                    // NH, 4.8.1 - above replaced by loading the right media file from the db later!
                    orgSrc =
						Umbraco.Core.IO.IOHelper.ResolveUrl(orgSrc.Replace("%20", " "));

                    // Check for either id or guid from media
                    string mediaId = getIdFromSource(orgSrc, rootMediaUrl);

                    Media imageMedia = null;

                    try
                    {
                        int mId = int.Parse(mediaId);
                        Property p = new Property(mId);
                        imageMedia = new Media(Content.GetContentFromVersion(p.VersionId).Id);
                    }
                    catch
                    {
                        try
                        {
                            imageMedia = new Media(Content.GetContentFromVersion(new Guid(mediaId)).Id);
                        }
                        catch
                        {
                        }
                    }

                    // Check with the database if any media matches this url
                    if (imageMedia != null)
                    {
                        try
                        {
                            // Format the tag
                            tempTag = tempTag + " rel=\"" +
                                      imageMedia.getProperty("umbracoWidth").Value.ToString() + "," +
									  imageMedia.getProperty("umbracoHeight").Value.ToString() + "\" src=\"" + Umbraco.Core.IO.IOHelper.ResolveUrl(imageMedia.getProperty("umbracoFile").Value.ToString()) +
                                      "\"";
                            tempTag += "/>";

                            // Replace the tag
                            html = html.Replace(tag.Value, tempTag);
                        }
                        catch (Exception ee)
                        {
                            Log.Add(LogTypes.Error, User.GetUser(0), -1,
                                    "Error reading size data from media: " + imageMedia.Id.ToString() + ", " +
                                    ee.ToString());
                        }
                    }
                    else
                        Log.Add(LogTypes.Error, User.GetUser(0), -1,
                                "Error reading size data from media (not found): " + orgSrc);
                }
            return html;
        }

        private string getIdFromSource(string src, string rootMediaUrl)
        {
            if (!rootMediaUrl.EndsWith("/"))
                rootMediaUrl += "/";

            // important - remove out the rootMediaUrl!
            src = src.Replace(rootMediaUrl, "");

            string _id = "";

            // Check for directory id naming 
            if (src.Contains("/"))
            {
                string[] dirSplit = src.Split("/".ToCharArray());
                string tempId = dirSplit[0];
                try
                {
                    // id
                    _id = int.Parse(tempId).ToString();
                }
                catch
                {
                    // guid
                    _id = tempId;
                }
            }
            else
            {
                string[] fileSplit = src.Split("-".ToCharArray());

                // guid or id
                if (fileSplit.Length > 3)
                {
                    for (int i = 0; i < 5; i++)
                        _id += fileSplit[i] + "-";
                    _id = _id.Substring(0, _id.Length - 1);
                }
                else
                    _id = fileSplit[0];
            }

            return _id;
        }


        private string parseMacrosToHtml(string input)
        {
            int nodeId = _nodeId;
            Guid versionId = _versionId;
            string content = input;


            string pattern = @"(<\?UMBRACO_MACRO\W*[^>]*/>)";
            MatchCollection tags =
            Regex.Matches(content, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            // Page for macro rendering
            //          page p = new page(nodeId, versionId);


            System.Web.HttpContext.Current.Items["macrosAdded"] = 0;
            System.Web.HttpContext.Current.Items["pageID"] = nodeId.ToString();

            foreach (Match tag in tags)
            {
                try
                {
                    // Create div
                    Hashtable attributes = helper.ReturnAttributes(tag.Groups[1].Value);
                    string div = macro.renderMacroStartTag(attributes, nodeId, versionId); //.Replace("&quot;", "&amp;quot;");

                    // Insert macro contents here...
                    macro m;
                    if (helper.FindAttribute(attributes, "macroID") != "")
                        m = macro.GetMacro(int.Parse(helper.FindAttribute(attributes, "macroID")));
                    else
                    {
                        // legacy: Check if the macroAlias is typed in lowercasing
                        string macroAlias = helper.FindAttribute(attributes, "macroAlias");
                        if (macroAlias == "")
                        {
                            macroAlias = helper.FindAttribute(attributes, "macroalias");
                            attributes.Remove("macroalias");
                            attributes.Add("macroAlias", macroAlias);
                        }

                        if (macroAlias != "")
                            m = macro.GetMacro(macroAlias);
                        else
                            throw new ArgumentException("umbraco is unable to identify the macro. No id or macroalias was provided for the macro in the macro tag.", tag.Groups[1].Value);
                    }

                    if (helper.FindAttribute(attributes, "macroAlias") == "")
                        attributes.Add("macroAlias", m.Alias);


                    try
                    {
                        div += macro.MacroContentByHttp(nodeId, versionId, attributes);
                    }
                    catch
                    {
                        div += "<span style=\"color: green\">No macro content available for WYSIWYG editing</span>";
                    }


                    div += macro.renderMacroEndTag();
                    content = content.Replace(tag.Groups[1].Value, div);
                }
                catch (Exception ee)
                {
                    Log.Add(LogTypes.Error, this._nodeId, "Macro Parsing Error: " + ee.ToString());
                    string div = "<div class=\"umbMacroHolder mceNonEditable\"><p style=\"color: red\"><strong>umbraco was unable to parse a macro tag, which means that parts of this content might be corrupt.</strong> <br /><br />Best solution is to rollback to a previous version by right clicking the node in the tree and then try to insert the macro again. <br/><br/>Please report this to your system administrator as well - this error has been logged.</p></div>";
                    content = content.Replace(tag.Groups[1].Value, div);
                }

            }
            return content;
        }
    }
}
