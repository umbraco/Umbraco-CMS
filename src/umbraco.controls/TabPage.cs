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

    public class TabPage : WebControl
    {
        // Ensure that a TabPage cannot be instatiated outside 
        // this assembly -> New instances of a tabpage can only be retrieved through the tabview
        private bool _hasMenu = true;
        private readonly ScrollingMenu _menu = new ScrollingMenu();
        protected LiteralControl ErrorHeaderControl = new LiteralControl();
        private LiteralControl _closeButtonControl = new LiteralControl();
        private readonly ValidationSummary _vs = new ValidationSummary();
        private readonly Control _tempErr = new Control();
        internal TabView parent;


        public string Text { get; set; }

        internal TabPage()
        {

        }

        public ValidationSummary ValidationSummaryControl
        {
            get { return _vs; }
        }

        public string ErrorHeader { get; set; }
        public string CloseCaption { get; set; }
        public bool Active { get; set; }

        public Control ErrorControl
        {
            get { return _tempErr; }
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
            _tempErr.Controls.Add(_closeButtonControl);
            _tempErr.Controls.Add(ErrorHeaderControl);
            _tempErr.Controls.Add(new LiteralControl("</h3><p>"));
            _tempErr.Controls.Add(_vs);
            _tempErr.Controls.Add(new LiteralControl("</p></div>"));

            this.Controls.Add(_tempErr);
        }

        public ScrollingMenu Menu
        {
            get { return parent.Menu; }
        }

        public bool HasMenu
        {
            get { return _hasMenu; }
            set { _hasMenu = value; }
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            _closeButtonControl.Text = "<div id=\"errorPane_" + this.ClientID + "\" style='margin: 10px 0px 10px 0px; text-align: left;' class=\"error\"><div style=\"float: right; padding: 5px;\"><a href=\"#\" onClick=\"javascript:document.getElementById('errorPane_" + this.ClientID + "').style.display = 'none'; return false;\">" + CloseCaption + "</a></div><h3>";
            ErrorHeaderControl.Text = ErrorHeader;
           
            var activeClass = string.Empty;

            if (this.ID == parent.ActiveTabId)
                activeClass = "active";

            writer.WriteLine("<div id='" + this.ID + "' class='umb-tab-pane tab-pane form-horizontal " + activeClass + " " + parent.ActiveTabId + "'>");
            writer.WriteLine("<div class='umb-tab-pane-inner' id='" + this.ClientID + "_contentlayer'>");

            this.RenderChildren(writer);

            writer.WriteLine("</div>");
            writer.WriteLine("</div>");
        }
    }
}
