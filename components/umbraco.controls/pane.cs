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
using umbraco.presentation.ClientDependency;

namespace umbraco.uicontrols {

	[ClientDependency(ClientDependencyType.Css, "propertypane/style.css", "UmbracoClient")]
    public class Pane : System.Web.UI.WebControls.Panel {

        private TableRow tr = new TableRow();
        private TableCell td = new TableCell();
        private Table tbl = new Table();

        public Pane() {

        }

        private string m_Text = string.Empty;
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }


        public void addProperty(string Caption, Control C) {
            /*
            tr = new TableRow();
            td = new TableCell();
            td.Text = Caption;
            td.Attributes.Add("width", "15%");
            td.Attributes.Add("valign", "top");
            td.Attributes.Add("class", "propertyHeader");
            tr.Cells.Add(td);
                        
            td = new TableCell();
            td.Attributes.Add("class", "propertyContent");
            td.Controls.Add(C);
            tr.Cells.Add(td);

            tbl.Rows.Add(tr);
            if (!this.Controls.Contains(tbl)) {
                this.Controls.Add(tbl);
            }*/
            PropertyPanel pp = new PropertyPanel();
            pp.Controls.Add(C);
            pp.Text = Caption;

            this.Controls.Add(pp);
        }

        public void addProperty(Control ctrl) {
            /*
            tr = new TableRow();
            td = new TableHeaderCell();
            td.ColumnSpan = 2;
            td.Attributes.Add("width", "100%");
            td.Attributes.Add("valign", "top");
            td.Attributes.Add("class", "propertyContent");
            td.Controls.Add(ctrl);
            tr.Cells.Add(td);
            tbl.Rows.Add(tr);
            if (!this.Controls.Contains(tbl)) {
                this.Controls.Add(tbl);
            }
             */

            PropertyPanel pp = new PropertyPanel();
            pp.Controls.Add(ctrl);
            this.Controls.Add(pp);
       }


        protected override void OnLoad(System.EventArgs EventArguments) {

        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer) {

            this.CreateChildControls();
            string styleString = "";

            foreach (string key in this.Style.Keys) {
                styleString += key + ":" + this.Style[key] + ";";
            }

            if (!string.IsNullOrEmpty(m_Text))
                writer.WriteLine("<h2 class=\"propertypaneTitel\">" + m_Text + "</h2>");
            
            writer.WriteLine("<div class=\"propertypane\" style='" + styleString + "'>");
            writer.WriteLine("<div>");

            try {
                this.RenderChildren(writer);
            } catch (Exception ex) {
                writer.WriteLine("Error creating control <br />");
                writer.WriteLine(ex.ToString());
            }

            writer.WriteLine("<div class='propertyPaneFooter'>-</div></div></div>");

        }

    }

}