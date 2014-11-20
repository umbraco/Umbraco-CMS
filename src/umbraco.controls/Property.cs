﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace umbraco.uicontrols
{
    public class PropertyPanel : System.Web.UI.WebControls.Panel
    {
        public PropertyPanel()
        {

        }

        private string m_Text = string.Empty;
        private Control _control;

        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
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

            var inGroup = this.Parent.GetType() == typeof(PropertyGroup);

            if (string.IsNullOrEmpty(m_Text))
                CssClass += " hidelabel";

          
            writer.WriteLine("<div class=\"umb-el-wrap " + CssClass + "\">");


            if (m_Text != string.Empty)
            {
                if (_control == null)
                {
                    _control = Controls.OfType<Control>().FirstOrDefault(c => c.Visible && (c.GetType().Name.Contains("Literal") == false));
                }

                if (_control == null)
                {
                    writer.WriteLine("<span class=\"control-label\">{0}</span>", m_Text);
                }
                else
                {
                    writer.WriteLine("<label class=\"control-label\" for=\"{0}\">{1}</label>", _control != null ? _control.ClientID : "", m_Text);
                }
            }

            writer.WriteLine("<div class=\"controls controls-row\">");

            try
            {
                this.RenderChildren(writer);
            }
            catch (Exception ex)
            {
                writer.WriteLine("Error creating control <br />");
                writer.WriteLine(ex.ToString());
            }

            writer.WriteLine("</div></div>");

        }

        public void SetLabelFor(Control control)
        {
            _control = control;
        }
    }

}