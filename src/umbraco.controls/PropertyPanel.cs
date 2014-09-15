using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace umbraco.uicontrols
{
    public class PropertyPanel : System.Web.UI.WebControls.Panel
    {
        public PropertyPanel()
        {

        }

        private string _text = string.Empty;
        public string Text
        {
            get { return _text; }
            set { _text = value; }
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

            if (string.IsNullOrEmpty(_text))
                CssClass += " hidelabel";

          
            writer.WriteLine("<div class=\"umb-el-wrap " + CssClass + "\">");


            if (_text != string.Empty)
            {
                writer.WriteLine("<label class=\"control-label\" for=\"inputPassword\">" + _text + "</label>");
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

    }

}