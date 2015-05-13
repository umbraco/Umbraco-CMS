using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace umbraco.uicontrols
{
    public class PropertyPanel : System.Web.UI.WebControls.Panel
    {
        private Control _control;
        private string _text = string.Empty;

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
            
        protected override void OnLoad(EventArgs eventArguments)
        {
        }

        protected override void Render(HtmlTextWriter writer)
        {
            ViewStateMode = ViewStateMode.Disabled;
            CreateChildControls();


            if (string.IsNullOrEmpty(_text))
                CssClass += " hidelabel";

          
            writer.WriteLine("<div class=\"umb-el-wrap " + CssClass + "\">");


            if (_text != string.Empty)
            {
                if (_control == null)
                {
                    _control = Controls.OfType<Control>().FirstOrDefault(c => c.Visible && (c.GetType().Name.Contains("Literal") == false));
                }

                if (_control == null)
                {
                    writer.WriteLine("<span class=\"control-label\">{0}</span>", _text);
                }
                else
                {
                    writer.WriteLine("<label class=\"control-label\" for=\"{0}\">{1}</label>", _control != null ? _control.ClientID : "", _text);
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

    }

}