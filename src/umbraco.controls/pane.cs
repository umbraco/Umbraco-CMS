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

namespace umbraco.uicontrols
{

    public class Pane : System.Web.UI.WebControls.Panel
    {


        public Pane()
        {

        }

        private string m_Text = string.Empty;
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }

        private string m_title = string.Empty;
        public string Title
        {
            get { return m_title; }
            set { m_title = value; }
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


        protected override void OnLoad(System.EventArgs EventArguments)
        {

        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            this.ViewStateMode = ViewStateMode.Disabled;
            this.CreateChildControls();

            string styleString = "";

            foreach (string key in this.Style.Keys)
            {
                styleString += key + ":" + this.Style[key] + ";";
            }

            
            writer.WriteLine("<div class=\"umb-pane " + this.CssClass +  "\" style='" + styleString + "'>");
            if (!string.IsNullOrEmpty(m_title))
                writer.WriteLine("<h5 class='umb-pane-title'>" + m_title + "</h5>");
            writer.WriteLine("<div class=\"control-group umb-control-group\" style='" + styleString + "'>");

            if (!string.IsNullOrEmpty(m_Text))
                writer.WriteLine("<p class=\"umb-abstract\">" + m_Text + "</p>");

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
            writer.WriteLine("</div>");

        }

    }

}