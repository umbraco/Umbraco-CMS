using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.uicontrols {
    public class Feedback : System.Web.UI.WebControls.Panel {

        public Feedback() {

        }

        protected override void OnInit(EventArgs e) {
        }

        protected override void OnLoad(System.EventArgs EventArguments) {
        }

        private bool _hasMenu = false;
        private string _StatusBarText = "";

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

                writer.WriteLine("<div style=\"" + styleString + "\" class=\"" + type.ToString() + "\"><p>");
                writer.WriteLine(_text);
                writer.WriteLine("</p></div>");
            }  
        }
    }
}