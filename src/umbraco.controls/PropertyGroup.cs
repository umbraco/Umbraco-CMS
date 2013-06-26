using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace umbraco.uicontrols
{
    public class PropertyGroup : System.Web.UI.WebControls.Panel
    {
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            this.ViewStateMode = ViewStateMode.Disabled;
            this.CreateChildControls();
            string styleString = "";

            foreach (string key in this.Style.Keys)
            {
                styleString += key + ":" + this.Style[key] + ";";
            }

            writer.WriteLine("<div class=\"control-group umb-control-group\" style='" + styleString + "'>");

            try
            {
                this.RenderChildren(writer);
            }
            catch (Exception ex)
            {
                writer.WriteLine("Error creating control <br />");
                writer.WriteLine(ex.ToString());
            }

            writer.WriteLine("</div>");
        }

        public void addProperty(string Caption, Control C)
        {

            PropertyPanel pp = new PropertyPanel();
            pp.Controls.Add(C);
            pp.Text = Caption;

            this.Controls.Add(pp);
        }

        public void addProperty(Control ctrl)
        {

            PropertyPanel pp = new PropertyPanel();
            pp.Controls.Add(ctrl);
            this.Controls.Add(pp);
        }

    }
}
