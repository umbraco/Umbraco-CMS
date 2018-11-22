using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace umbraco.uicontrols
{
    internal class MenuSplitButton : Panel
    {

        public string Text { get; set; }
        public List<HtmlAnchor> HtmlAnchors { get; set; }

        public string OnClientClick { get; set; }    

        public void AddLink(HtmlAnchor link)
        {
            if(HtmlAnchors == null)
                HtmlAnchors = new List<HtmlAnchor>();

            HtmlAnchors.Add(link);
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write("</div>");
            base.Render(writer);
            this.Render(writer);
            writer.Write("<div class='btn-group>");
        }


        protected override void OnLoad(EventArgs e)
        {
 	        base.OnLoad(e);
            base.CssClass = "btn-group";

            HtmlButton btn = new HtmlButton();
            btn.InnerText = Text;
            btn.Attributes.Add("class", "btn");
            this.Controls.Add(btn);

            HtmlButton btnToggle = new HtmlButton();
            btnToggle.InnerHtml = "<span class='caret'></span>";
            btnToggle.Attributes.Add("class", "btn dropdown-toggl");
            btnToggle.Attributes.Add("data-toggle", "dropdown");
            this.Controls.Add(btnToggle);

            Literal list = new Literal();
            this.Controls.Add(list);

            StringBuilder sb = new StringBuilder();
            sb.Append("<ul class='dropdown-menu'>");

            foreach (var htmlAnchor in HtmlAnchors)
            {
                sb.Append("<li><a ");
                foreach (var attr in htmlAnchor.Attributes.Keys)
                {
                    sb.Append(attr + "='" + htmlAnchor.Attributes[attr.ToString()] + "' ");
                }
                sb.Append(">" + htmlAnchor.InnerText + "</a></li>");
            }

            sb.Append("</ul>");
            list.Text = sb.ToString();
        }

    }
}
