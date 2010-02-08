using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using ClientDependency.Core;
using System.Linq;
using ClientDependency.Core.Controls;
using umbraco.IO;

namespace umbraco.uicontrols {

    [ClientDependency(ClientDependencyType.Javascript, "CodeArea/javascript.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "CodeArea/UmbracoEditor.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "Application/jQuery/jquery-fieldselection.js", "UmbracoClient")]
    public class CodeArea : WebControl
    {

        public CodeArea()
        {
            //set the default to Css
            CodeBase = EditorType.Css;
        }

        protected TextBox CodeTextBox;

        public bool AutoResize { get; set ; }
        public int OffSetX { get; set; }
        public int OffSetY { get; set; }
        public string Text
        {
            get
            {
                EnsureChildControls();
                return CodeTextBox.Text;
            }
            set
            {
                EnsureChildControls();
                CodeTextBox.Text = value;
            }
        }

        public EditorType CodeBase { get; set; }
        public string ClientSaveMethod { get; set; }

        public enum EditorType { JavaScript, Css, Python, XML, HTML }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();

            if (!UmbracoSettings.ScriptDisableEditor)
            {
                ClientDependencyLoader.Instance.RegisterDependency("CodeMirror/js/codemirror.js", "UmbracoClient", ClientDependencyType.Javascript);
                ClientDependencyLoader.Instance.RegisterDependency("CodeArea/styles.css", "UmbracoClient", ClientDependencyType.Css);
            }        
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            CodeTextBox = new TextBox();
            CodeTextBox.ID = "CodeTextBox";

            if (UmbracoSettings.ScriptDisableEditor)
            {
                CodeTextBox.Attributes.Add("class", "codepress");
                CodeTextBox.Attributes.Add("wrap", "off");
            }
            else            
            
            CodeTextBox.TextMode = TextBoxMode.MultiLine;

            this.CssClass = "codepress";

            this.Controls.Add(CodeTextBox);

        }

        /// <summary>
        /// Client ID is different if the code editor is turned on/off
        /// </summary>
        public override string ClientID
        {
            get
            {
                if (UmbracoSettings.ScriptDisableEditor)
                    return CodeTextBox.ClientID;
                else
                    return base.ClientID;
            }
        }

        protected override void Render(HtmlTextWriter writer) 
        {
            EnsureChildControls();

            var jsEventCode = "";
            if (UmbracoSettings.ScriptDisableEditor)
            {
                CodeTextBox.RenderControl(writer);
                jsEventCode = RenderBasicEditor();
            }
            else
            {                
                writer.WriteBeginTag("div");
                writer.WriteAttribute("id", this.ClientID);
                writer.WriteAttribute("class", this.CssClass);
                this.ControlStyle.AddAttributesToRender(writer);
                writer.Write(HtmlTextWriter.TagRightChar);            
                CodeTextBox.RenderControl(writer);
                writer.WriteEndTag("div");

                jsEventCode = RenderCodeEditor();
            }
                


            if (this.AutoResize)
            {
                if (!UmbracoSettings.ScriptDisableEditor)
                {
                    //reduce the width if using code mirror because of the line numbers
                    OffSetX += 20;
                }

                jsEventCode += @"                    

                    //create the editor
                   var UmbEditor = new Umbraco.Controls.CodeEditor.UmbracoEditor(" + UmbracoSettings.ScriptDisableEditor.ToString().ToLower() + @", '" + this.ClientID + @"');


                    var m_textEditor = jQuery('#" + this.ClientID + @"');
                    //with codemirror adding divs for line numbers, we need to target a different element
                    m_textEditor = m_textEditor.find('iframe').length > 0 ? m_textEditor.children('div').get(0) : m_textEditor.get(0);
                    jQuery(window).resize(function(){  resizeTextArea(m_textEditor, " + OffSetX.ToString() + "," + OffSetY.ToString() + @"); });
            		jQuery(document).ready(function(){  resizeTextArea(m_textEditor, " + OffSetX.ToString() + "," + OffSetY.ToString() + @"); });
                    ";
            }

            jsEventCode = string.Format(@"<script type=""text/javascript"">{0}</script>", jsEventCode);
            writer.WriteLine(jsEventCode);


        }

        protected string RenderBasicEditor()
        {
            string jsEventCode = @"                                   
                                    var m_textEditor = document.getElementById('" + this.ClientID + @"');                                   
                                    tab.watch('" + this.ClientID + @"');
                                    ";
            return jsEventCode;
        }

        protected string RenderCodeEditor()
        {

            string[] parserFiles = new string[] { "tokenizejavascript.js", "parsejavascript.js" };
            string[] cssFile = new string[] { "jscolors.css", "umbracoCustom.css" };

            switch (CodeBase)
            {
                case EditorType.JavaScript:
                    parserFiles = new string[] { "tokenizejavascript.js", "parsejavascript.js" };
                    cssFile = new string[] { "jscolors.css", "umbracoCustom.css" };
                    break;
                case EditorType.Css:
                    parserFiles = new string[] { "parsecss.js" };
                    cssFile = new string[] { "csscolors.css", "umbracoCustom.css" };
                    break;
                case EditorType.Python:
                    parserFiles = new string[] { "parsepython.js" };
                    cssFile = new string[] { "pythoncolors.css", "umbracoCustom.css" };
                    break;
                case EditorType.XML:
                    parserFiles = new string[] { "parsexml.js" };
                    cssFile = new string[] { "xmlcolors.css", "umbracoCustom.css" };
                    break;
                case EditorType.HTML:
                    parserFiles = new string[] { "parsexml.js", "parsecss.js", "tokenizejavascript.js", "parsejavascript.js", "parsehtmlmixed.js" };
                    cssFile = new string[] { "xmlcolors.css", "jscolors.css", "csscolors", "umbracoCustom.css" };
                    break;
            }

            var jsEventCode = @"                              
                            var textarea = document.getElementById('" + CodeTextBox.ClientID + @"');
                            

                              var codeEditor = CodeMirror.fromTextArea(textarea, {

                                width: ""100%"",
                                height: ""100%"",
                                tabMode: ""shift"",
                                textWrapping: false,
                                lineNumbers: true,
                                parserfile: [" + string.Join(",",
                                               parserFiles
                                                    .Select(x => string.Format(@"""{0}""", x))
                                                    .ToArray()) + @"],
                                stylesheet: [" + string.Join(",",

                                               cssFile
                                                    .Select(x => string.Format(@"""{0}""", IOHelper.ResolveUrl( SystemDirectories.Umbraco_client ) + @"/CodeMirror/css/" + x))
                                                    .ToArray()) + @"],
                                path: """ + IOHelper.ResolveUrl( SystemDirectories.Umbraco_client) + @"/CodeMirror/js/"",
                                content: textarea.value,             
                                autoMatchParens: false,"
                                    + (string.IsNullOrEmpty(ClientSaveMethod) ? "" : @"saveFunction: " + ClientSaveMethod + ",") + @"
                                onChange: function() { /*codeVal.value = codeEditor.getCode(); */}});

                                ";

            return jsEventCode;
        }
    }

}
