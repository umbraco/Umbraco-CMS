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

namespace umbraco.uicontrols {


	[ClientDependency(ClientDependencyType.Javascript, "panel/javascript.js", "UmbracoClient")]
	[ClientDependency(ClientDependencyType.Css, "panel/style.css", "UmbracoClient")]
   public class UmbracoPanel : System.Web.UI.WebControls.Panel {
        private ScrollingMenu _menu = new ScrollingMenu();

        public UmbracoPanel() {

        }

        protected override void OnInit(EventArgs e) {
            setupMenu();
        }

        protected override void OnLoad(System.EventArgs EventArguments) {

            _menu.Visible = hasMenu;

            if(_autoResize)
                this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "PanelEvents", "<script type='text/javascript'>jQuery(document).ready(function() {jQuery(window).load(function(){ resizePanel('" + this.ClientID + "', " + this.hasMenu.ToString().ToLower() + ",true); }) });</script>");
                //this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "PanelEvents", "<script type='text/javascript'>addEvent(window, \"load\", function(){ resizePanel('" + this.ClientID + "', " + this.hasMenu.ToString().ToLower() + "); }); addEvent(window, \"resize\", function(){ resizePanel('" + this.ClientID + "', " + this.hasMenu.ToString().ToLower() + "); });</script>");
        }

        private bool _hasMenu = false;
        private string _StatusBarText = "";
        private string _text;
        private bool _autoResize = true;

        public bool hasMenu {
            get { return _hasMenu; }
            set { _hasMenu = value; }
        }

        public bool AutoResize {
            get { return _autoResize; }
            set { _autoResize = value; }
        }

        public string Text {
            get {
                if (_text == "") {
                    _text = "&nbsp;";
                }
                return _text;

            }
            set { _text = value; }
        }

        public string StatusBarText {
            get { return _StatusBarText; }
            set { _StatusBarText = value; }
        }

        public ScrollingMenu Menu {
            get { return _menu; }
        }

        internal void setupMenu() {
                _menu.ID = this.ID + "_menu";
                if (this.Width.Value < 20)
                    this.Width = Unit.Pixel(24);
                _menu.Width = (int)Unit.Pixel((int)this.Width.Value - 20).Value;
                this.Controls.Add(_menu);

        }
       
        protected override void Render(System.Web.UI.HtmlTextWriter writer) {
            base.CreateChildControls();
            
            try {
                if (System.Web.HttpContext.Current == null) {
                    writer.WriteLine("Number of child controls : " + this.Controls.Count);
                }
                writer.WriteLine("<div id=\"" + this.ClientID + "\" class=\"panel\" style=\"height:" + this.Height.Value + "px;width:" + this.Width.Value + "px;\">");
                writer.WriteLine("<div class=\"boxhead\">");
                writer.WriteLine("<h2 id=\"" + this.ClientID + "Label\">" + this.Text + "</h2>");
                writer.WriteLine("</div>");
                writer.WriteLine("<div class=\"boxbody\">");
                
                if (this.hasMenu) {
                    writer.WriteLine("<div id='" + this.ClientID + "_menubackground' class=\"menubar_panel\">");
                    _menu.RenderControl(writer);
                    writer.WriteLine("</div>");
                }

                int upHeight = (int)this.Height.Value - 46;
                int upWidth = (int)this.Width.Value - 5;

                if (this.hasMenu)
                    upHeight = upHeight - 34;
                
                writer.WriteLine("<div id=\"" + this.ClientID + "_content\" class=\"content\" style=\"width: auto; height:" + (upHeight) + "px;\">");

                string styleString = "";

                foreach (string key in this.Style.Keys) {
                    styleString += key + ":" + this.Style[key] + ";";
                }

                writer.WriteLine("<div class=\"innerContent\" id=\"" + this.ClientID + "_innerContent\" style='" + styleString + "'>");
                foreach (Control c in this.Controls) {
                    if (!(c.ID == _menu.ID)) {
                        c.RenderControl(writer);
                    }
                }

                writer.WriteLine("</div>");
                writer.WriteLine("</div>");
                writer.WriteLine("</div>");
                writer.WriteLine("<div class=\"boxfooter\"><div class=\"statusBar\"><h2>" + this.StatusBarText + "</h2></div></div>");
                writer.WriteLine("</div>");

                /*
                if(_autoResize)
                    writer.WriteLine("<script type=\"text/javascript\">jQuery(document).ready(function(){ resizePanel('" + this.ClientID + "', " + this.hasMenu.ToString().ToLower() + ");});</script>");
                */

            } catch (Exception ex) {
                this.Page.Trace.Warn("Error rendering umbracopanel control" + ex.ToString());
            }
        }
    }
}