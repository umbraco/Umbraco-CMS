using System;
using System.Collections.Generic;
using System.Text;
using ClientDependency.Core;

namespace umbraco.uicontrols {
    
    [ClientDependency(ClientDependencyType.Css, "ui/default.css", "UmbracoClient")]
    public class Feedback : System.Web.UI.WebControls.Panel 
    {

        public Feedback() {

        }

        protected override void OnInit(EventArgs e) {
        }

        protected override void OnLoad(System.EventArgs EventArguments) {
        }

        public feedbacktype type { get; set; }

        private string _text = string.Empty;
        public string Text {
            get {
              
                return _text;

            }
            set { _text = value; }
        }

        public enum feedbacktype{
            notice,
            error,
            success
        }
       
        protected override void Render(System.Web.UI.HtmlTextWriter writer) {
            if (_text != string.Empty) {
                base.CreateChildControls();

                string styleString = "";
                foreach (string key in this.Style.Keys) {
                    styleString += key + ":" + this.Style[key] + ";";
                }

                writer.WriteLine("<div id=\"" + this.ClientID + "\" style=\"" + styleString + "\" class=\"alert alert-" + type.ToString() + "\"><p>");
                writer.WriteLine(_text);
                writer.WriteLine("</p></div>");
            }  
        }
    }
}