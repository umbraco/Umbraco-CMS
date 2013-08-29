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

namespace umbraco.uicontrols
{

    [ClientDependency(ClientDependencyType.Javascript, "CodeArea/javascript.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "CodeArea/UmbracoEditor.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "CodeArea/styles.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "Application/jQuery/jquery-fieldselection.js", "UmbracoClient")]
    public class CodeArea : WebControl
    {

        public CodeArea()
        {
            //set the default to Css
            CodeBase = EditorType.Css;
        }

        protected TextBox CodeTextBox;

        public bool AutoResize { get; set; }
        public bool AutoSuggest { get; set; }
        public string EditorMimeType { get; set; }

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

        public bool CodeMirrorEnabled
        {
            get
            {
                return UmbracoSettings.ScriptDisableEditor == false;
            }
        }

        public EditorType CodeBase { get; set; }
        public string ClientSaveMethod { get; set; }

        public enum EditorType { JavaScript, Css, Python, XML, HTML, Razor, HtmlMixed }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();

            if (CodeMirrorEnabled)
            {
                ClientDependencyLoader.Instance.RegisterDependency(0, "CodeMirror/js/lib/codemirror.js", "UmbracoClient", ClientDependencyType.Javascript);


                ClientDependencyLoader.Instance.RegisterDependency(2, "CodeMirror/js/mode/" + CodeBase.ToString().ToLower() + "/" + CodeBase.ToString().ToLower() + ".js", "UmbracoClient", ClientDependencyType.Javascript);
                if (CodeBase == EditorType.HtmlMixed)
                {
                    ClientDependencyLoader.Instance.RegisterDependency(1, "CodeMirror/js/mode/xml/xml.js", "UmbracoClient", ClientDependencyType.Javascript);
                    ClientDependencyLoader.Instance.RegisterDependency(1, "CodeMirror/js/mode/javascript/javascript.js", "UmbracoClient", ClientDependencyType.Javascript);
                    ClientDependencyLoader.Instance.RegisterDependency(1, "CodeMirror/js/mode/css/css.js", "UmbracoClient", ClientDependencyType.Javascript);
                }

                ClientDependencyLoader.Instance.RegisterDependency(2, "CodeMirror/js/lib/util/search.js", "UmbracoClient", ClientDependencyType.Javascript);
                ClientDependencyLoader.Instance.RegisterDependency(2, "CodeMirror/js/lib/util/searchcursor.js", "UmbracoClient", ClientDependencyType.Javascript);
                ClientDependencyLoader.Instance.RegisterDependency(2, "CodeMirror/js/lib/util/dialog.js", "UmbracoClient", ClientDependencyType.Javascript);
                ClientDependencyLoader.Instance.RegisterDependency(2, "CodeMirror/js/lib/util/dialog.css", "UmbracoClient", ClientDependencyType.Css);

                ClientDependencyLoader.Instance.RegisterDependency(2, "CodeMirror/js/lib/codemirror.css", "UmbracoClient", ClientDependencyType.Css);
                ClientDependencyLoader.Instance.RegisterDependency(3, "CodeMirror/css/umbracoCustom.css", "UmbracoClient", ClientDependencyType.Css);
                ClientDependencyLoader.Instance.RegisterDependency(4, "CodeArea/styles.css", "UmbracoClient", ClientDependencyType.Css);
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            CodeTextBox = new TextBox();
            CodeTextBox.ID = "CodeTextBox";

            if (CodeMirrorEnabled == false)
            {
                CodeTextBox.Attributes.Add("class", "codepress");
                CodeTextBox.Attributes.Add("wrap", "off");
            }

            CodeTextBox.TextMode = TextBoxMode.MultiLine;
            
            this.Controls.Add(CodeTextBox);

        }

        /// <summary>
        /// Client ID is different if the code editor is turned on/off
        /// </summary>
        public override string ClientID
        {
            get
            {
                if (CodeMirrorEnabled == false)
                    return CodeTextBox.ClientID;
                else
                    return base.ClientID;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            EnsureChildControls();

            var jsEventCode = "";


            if (CodeMirrorEnabled == false)
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
                if (CodeMirrorEnabled)
                {
                    //reduce the width if using code mirror because of the line numbers
                    OffSetX += 20;
                }

                jsEventCode += @"   
					//TODO: for now this is a global var, need to refactor all this so that is using proper js standards
					//with correct objects, and proper accessors to these objects.
					var UmbEditor;                 
                    $(document).ready(function () {
                        //create the editor
                       UmbEditor = new Umbraco.Controls.CodeEditor.UmbracoEditor(" + (CodeMirrorEnabled == false).ToString().ToLower() + @", '" + this.ClientID + @"');
                       var m_textEditor = jQuery('#" + this.ClientID + @"');
                   
                       //with codemirror adding divs for line numbers, we need to target a different element
                       m_textEditor = m_textEditor.find('iframe').length > 0 ? m_textEditor.children('div').get(0) : m_textEditor.get(0);
                   
                       jQuery(window).resize(function(){  resizeTextArea(m_textEditor, " + OffSetX.ToString() + "," + OffSetY.ToString() + @"); });
            	       jQuery(document).ready(function(){  resizeTextArea(m_textEditor, " + OffSetX.ToString() + "," + OffSetY.ToString() + @"); });
                    });";
  
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
            var extraKeys = "";
            var editorMimetype = "";

            if (string.IsNullOrEmpty(EditorMimeType) == false)
                editorMimetype = @", 
                                     mode: """ + EditorMimeType + "\"";



            var jsEventCode = @"
                                var textarea = document.getElementById('" + CodeTextBox.ClientID + @"');
                                var codeEditor = CodeMirror.fromTextArea(textarea, {
                                                width: ""100%"",
                                                height: ""100%"",
                                                tabMode: ""shift"",
                                                matchBrackets: true,
                                                indentUnit: 4,
                                                indentWithTabs: true,
                                                enterMode: ""keep"",
                                                onCursorActivity: updateLineInfo,
                                                lineWrapping: false" +
                                                editorMimetype + @",
                                                lineNumbers: true" +
                                                extraKeys + @"
                                                });
                                  
                                    updateLineInfo(codeEditor);
                                ";

            return jsEventCode;
        }
    }

}
