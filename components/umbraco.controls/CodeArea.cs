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

namespace umbraco.uicontrols {

    public class CodeArea : System.Web.UI.WebControls.TextBox {

        public CodeArea() {
            this.Attributes.Add("class", "codepress");
            this.Attributes.Add("onclick", "storeCaret(this)");
            this.Attributes.Add("onselect", "storeCaret(this)");
            this.Attributes.Add("onkeyup", "storeCaret(this)");

            this.Attributes.Add("wrap", "off");
            this.TextMode = TextBoxMode.MultiLine;
        }


        public bool AutoResize { get; set ; }
        public int OffSetX { get; set; }
        public int OffSetY { get; set; }

        protected override void OnInit(System.EventArgs e) {


           //this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "CodeAreaStyles", "<link rel='stylesheet' href='/umbraco_client/CodeArea/style.css' />");
            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "CodeAreaJavaScript", "<script language='javascript' src='/umbraco_client/CodeArea/javascript.js'></script>");
           // this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "CodeAreaJavaScript_new", "<script language='javascript' src='/umbraco_client/CodeArea/xbf.js'></script>");
            

        }

        protected override void Render(HtmlTextWriter writer) {
            base.Render(writer);

            string jsEventCode = @"<script type='text/javascript'>
                                    var m_textEditor = document.getElementById('" + this.ClientID + @"');
                                    if (navigator.userAgent.match('MSIE')) {
                                        //addEvent(m_textEditor, ""select"", function() { storeCaret(this); });
		                                //addEvent(m_textEditor, ""click"", function() { storeCaret(this); });
                                        //addEvent(m_textEditor, ""keyup"", function() { storeCaret(this); });
                                    }
                                    
                                    tab.watch('" + this.ClientID + @"');
                                    ";
            if (this.AutoResize) {
                jsEventCode += @"   
                                    jQuery(window).resize(function(){  resizeTextArea(m_textEditor, " + OffSetX.ToString() + "," + OffSetY.ToString() + @"); });
		                            jQuery(document).ready(function(){  resizeTextArea(m_textEditor, " + OffSetX.ToString() + "," + OffSetY.ToString() + @"); });
                                    ";
            }

            jsEventCode += "</script>";

            writer.WriteLine(jsEventCode);

            //this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "CodeAreaEvents", jsEventCode);
        }
    }

}