using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.umbraco.controls
{
    public class LabelButton : System.Web.UI.WebControls.ImageButton
    {
        protected System.Web.UI.WebControls.LinkButton caption = new LinkButton();

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected void caption_Click(object sender, EventArgs e)
        {
            // we provide the click event using bogus x/y coords
            this.OnClick(new ImageClickEventArgs(1, 1));
        }

        public override ControlCollection Controls
        {
            get
            {
                EnsureChildControls();
                return base.Controls;
            }
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();
            caption.Click += new EventHandler(caption_Click);
            caption.OnClientClick = this.OnClientClick;
            caption.Text = base.ToolTip;
            caption.ToolTip = base.ToolTip;
            Controls.Add(caption);
        }

        public override string OnClientClick
        {
            get
            {
                return base.OnClientClick;
            }
            set
            {
                base.OnClientClick = value;
                caption.OnClientClick = value;
            }
        }


        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {

            writer.WriteBeginTag("div");
            writer.WriteAttribute("class", "umbLabelButton");
            writer.Write(HtmlTextWriter.TagRightChar);
            base.Render(writer);
            caption.RenderControl(writer);
            writer.WriteEndTag("div");
        }
    }
}
