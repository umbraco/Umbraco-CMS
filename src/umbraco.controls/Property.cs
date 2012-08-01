using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.uicontrols {
    public class PropertyPanel  : System.Web.UI.WebControls.Panel {
        public PropertyPanel() {

        }

        private string m_Text = string.Empty;
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }
                

        protected override void OnLoad(System.EventArgs EventArguments) {
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer) {

            this.CreateChildControls();
            string styleString = "";

            foreach (string key in this.Style.Keys) {
                styleString += key + ":" + this.Style[key] + ";";
            }

            writer.WriteLine("<div class=\"propertyItem\" style='" + styleString + "'>");
            if (m_Text != string.Empty) {
                writer.WriteLine("<div class=\"propertyItemheader\">" + m_Text + "</div>");
                writer.WriteLine("<div class=\"propertyItemContent\">");
            }

            try {
                this.RenderChildren(writer);
            } catch (Exception ex) {
                writer.WriteLine("Error creating control <br />");
                writer.WriteLine(ex.ToString());
            }

            if (m_Text != string.Empty)
                writer.WriteLine("</div>");

            writer.WriteLine("</div>");
            
            
        }

    }

}