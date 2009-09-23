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

namespace umbraco.uicontrols {

    [ClientDependency(ClientDependencyType.Javascript, "CodeArea/resizeTextEditor.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "CodeArea/UmbracoEditor.js", "UmbracoClient")]
    public class CodeArea : WebControl
    {

        public CodeArea()
        {
            //set the default to Css
            CodeBase = EditorType.Css;
        }

        protected TextBox CodeTextBox;
        protected HiddenField CodeEditorValue;

        /// <summary>
        /// Used to track the postback event, this updates the hidden field's value so that
        /// on postback the value is returned.
        /// </summary>
        protected CustomValidator UpdateCodeValueValidator;

        public bool AutoResize { get; set ; }
        public int OffSetX { get; set; }
        public int OffSetY { get; set; }
        public string Text
        {
            get
            {
                return CodeEditorValue.Value;
            }
            set
            {
                CodeEditorValue.Value = value;
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
            
            ClientDependencyLoader.Instance.RegisterDependency("CodeArea/javascript.js", "UmbracoClient", ClientDependencyType.Javascript);
        
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            CodeEditorValue = new HiddenField();
            CodeEditorValue.ID = "CodeEditorValue";
            CodeTextBox = new TextBox();
            CodeTextBox.ID = "CodeTextBox";

            if (UmbracoSettings.ScriptDisableEditor)
            {
                CodeTextBox.Attributes.Add("class", "codepress");
                CodeTextBox.Attributes.Add("onclick", "storeCaret(this)");
                CodeTextBox.Attributes.Add("onselect", "storeCaret(this)");
                CodeTextBox.Attributes.Add("onkeyup", "storeCaret(this)");
                CodeTextBox.Attributes.Add("wrap", "off");
            }
            else
            {
                UpdateCodeValueValidator = new CustomValidator();
                UpdateCodeValueValidator.ID = "UpdateCodeValueValidator";
                UpdateCodeValueValidator.ClientValidationFunction = "updateCodeEditorValue";
                UpdateCodeValueValidator.Display = ValidatorDisplay.None;
                this.Controls.Add(UpdateCodeValueValidator);
            }
            
            CodeTextBox.TextMode = TextBoxMode.MultiLine;

            this.CssClass = "codepress";

            this.Controls.Add(CodeEditorValue);            
            this.Controls.Add(CodeTextBox);

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            CodeTextBox.Text = CodeEditorValue.Value;
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
                UpdateCodeValueValidator.RenderControl(writer);
                CodeEditorValue.RenderControl(writer);
                
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
                                    if (navigator.userAgent.match('MSIE')) {
                                        //addEvent(m_textEditor, ""select"", function() { storeCaret(this); });
		                                //addEvent(m_textEditor, ""click"", function() { storeCaret(this); });
                                        //addEvent(m_textEditor, ""keyup"", function() { storeCaret(this); });
                                    }
                                    
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
                              var codeVal = document.getElementById('" + CodeEditorValue.ClientID + @"');
                              function updateCodeEditorValue(source, args) {
                                codeVal.value = codeEditor.getCode();
                                //alert(codeVal.value); 
                                args.IsValid = true;
                              }

                              var codeEditor = new CodeMirror(CodeMirror.replace(textarea), {
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
                                                    .Select(x => string.Format(@"""{0}""", GlobalSettings.ClientPath + @"/CodeMirror/css/" + x))
                                                    .ToArray()) + @"],
                                path: """ + GlobalSettings.ClientPath + @"/CodeMirror/js/"",
                                content: textarea.value,             
                                autoMatchParens: true," 
                                    + (string.IsNullOrEmpty(ClientSaveMethod) ? "" : @"saveFunction: " + ClientSaveMethod + ",") + @"
                                onChange: function() { /*codeVal.value = codeEditor.getCode(); */}});

                                ";

            return jsEventCode;
        }
    }

}