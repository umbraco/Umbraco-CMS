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

namespace umbraco.uicontrols {
    public class TabPage : System.Web.UI.WebControls.WebControl {
        // Ensure that a TabPage cannot be instatiated outside 
        // this assembly -> New instances of a tabpage can only be retrieved through the tabview
        private bool _hasMenu = true;
        private ScrollingMenu _Menu = new ScrollingMenu();
        protected LiteralControl ErrorHeaderControl = new LiteralControl();
        private ValidationSummary _vs = new ValidationSummary();
        private Control _tempErr = new Control();
        internal TabPage() {

        }

        public ValidationSummary ValidationSummaryControl
        {
            get { return _vs; }
        }

        public string ErrorHeader { get; set; }
        public string CloseCaption { get; set; }
        public Control ErrorControl { get { return _tempErr; } }

        protected override void OnLoad(EventArgs e) {
            if (this.HasMenu) {
                Menu.Width = System.Web.UI.WebControls.Unit.Pixel((int)this.Width.Value - 12);
                _Menu.ID = this.ID + "_menu";
                this.Controls.Add(_Menu);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _vs.ShowSummary = true;
            _vs.Attributes.Remove("style");
            _vs.Style.Clear();
            _vs.ForeColor = new Color();
            _vs.ID = String.Format("{0}_validationSummary", this.ID);

            // Add error pane
            _tempErr.Visible = false;
            _tempErr.ID = String.Format("{0}_errorPaneContainer", this.ID);
            _tempErr.Controls.Add(new LiteralControl("<div id=\"errorPane_" + this.ClientID + "\" style='margin: 10px 0px 10px 0px; text-align: left;' class=\"error\"><div style=\"float: right; padding: 5px;\"><a href=\"#\" onClick=\"javascript:document.getElementById('errorPane_" + this.ClientID + "').style.display = 'none'; return false;\">" + CloseCaption + "</a></div><h3>"));
            _tempErr.Controls.Add(ErrorHeaderControl);
            _tempErr.Controls.Add(new LiteralControl("</h3><p>"));
            _tempErr.Controls.Add(_vs);
            _tempErr.Controls.Add(new LiteralControl("</p></div>"));

            this.Controls.Add(_tempErr);
        }


        public ScrollingMenu Menu {
            get { return _Menu; }
        }

        public bool HasMenu {
            get { return _hasMenu; }
            set { _hasMenu = value; }
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer) {
            ErrorHeaderControl.Text = ErrorHeader;
            CreateChildControls();
            writer.WriteLine("<div id='" + this.ClientID + "' class='tabpage'>");
            if (HasMenu) {
                writer.WriteLine("<div class='menubar'>");
                Menu.Width = this.Width;
                Menu.RenderControl(writer);
                writer.WriteLine("</div>");
            }
            int ScrollingLayerHeight = (int)((System.Web.UI.WebControls.WebControl)this.Parent).Height.Value - 22;
            int ScrollingLayerWidth = (int)((System.Web.UI.WebControls.WebControl)this.Parent).Width.Value;
            if (HasMenu)
                ScrollingLayerHeight = ScrollingLayerHeight - 28;
            writer.WriteLine("<div class='tabpagescrollinglayer' id='" + this.ClientID + "_contentlayer' style='height:" + ScrollingLayerHeight + "px;width:" + ScrollingLayerWidth + "px'>");

            string styleString = "";
            foreach (string key in this.Style.Keys) {
                styleString += key + ":" + this.Style[key] + ";";
            }

            writer.WriteLine("<div class=\"tabpageContent\" style='" + styleString + "'>");

            _tempErr.RenderControl(writer);
            
            foreach (System.Web.UI.Control C in this.Controls) {
                if (C.ClientID != _Menu.ClientID && C.ClientID != _tempErr.ClientID ) {
                    C.RenderControl(writer);
                }
            }
            writer.WriteLine("</div>");
            writer.WriteLine("</div>");
            writer.WriteLine("</div>");
        }
    }
}
