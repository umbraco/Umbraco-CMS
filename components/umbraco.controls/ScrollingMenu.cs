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
    [ToolboxData("<{0}:ScrollingMenu runat=server></{0}:ScrollingMenu>")]
    public class ScrollingMenu : System.Web.UI.WebControls.WebControl {
        private ArrayList Icons = new ArrayList();
        private string iconIds;
        private int extraMenuWidth = 0;
        private string _ClientFilesPath = "/umbraco_client/scrollingmenu/";

        public MenuIconI NewIcon(int Index) {
            MenuIcon Icon = new MenuIcon();
            Icons.Insert(Index, Icon);
            return Icon;
        }

        public MenuIconI NewIcon() {
            MenuIcon icon = new MenuIcon();
            Icons.Add(icon);
            return icon;
        }

        public MenuImageButton NewImageButton() {
            MenuImageButton icon = new MenuImageButton();
            Icons.Add(icon);
            return icon;
        }

        public MenuImageButton NewImageButton(int Index) {
            MenuImageButton icon = new MenuImageButton();
            Icons.Insert(Index, icon);
            return icon;
        }

        public System.Web.UI.WebControls.DropDownList NewDropDownList() {
            DropDownList Icon = new DropDownList();
            Icons.Add(Icon);
            return Icon;
        }
        public void NewElement(string ElementName, string ElementId, string ElementClass, int ExtraWidth) {
            Icons.Add(new LiteralControl("<" + ElementName + " class=\"" + ElementClass + "\" id=\"" + ElementId + "\"></" + ElementName + ">"));
            extraMenuWidth = extraMenuWidth + ExtraWidth;
        }

        public void InsertSplitter() {
            Splitter icon = new Splitter();
            Icons.Add(icon);
        }
        public void InsertSplitter(int Index) {
            Splitter icon = new Splitter();
            Icons.Insert(Index, icon);
        }


        protected override void OnLoad(System.EventArgs EventArguments) {
            if (base.Visible)
            {
                SetupMenu();
                SetupClientScript();
            }
        }

        private void SetupClientScript() {
            helper.AddLinkToHeader("SCROLLINGMENUCSS", "/umbraco_client/scrollingmenu/style.css", this.Page);
            helper.AddScriptToHeader("SCROLLINGMENUJS", "/umbraco_client/scrollingmenu/javascript.js", this.Page);
//            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "SCROLLINGMENUCSS", "<link rel='stylesheet' href='/umbraco_client/scrollingmenu/style.css' />");
//            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "SCROLLINGMENUJS", "<script type='text/javascript' src='/umbraco_client/scrollingmenu/javascript.js'></script>");
        }

        private System.Web.UI.WebControls.Image scrollImage() {
            System.Web.UI.WebControls.Image functionReturnValue = null;
            functionReturnValue = new System.Web.UI.WebControls.Image();
            functionReturnValue.Width = Unit.Pixel(7);
            functionReturnValue.Height = Unit.Pixel(20);
            functionReturnValue.BorderWidth = Unit.Pixel(0);
            functionReturnValue.Attributes.Add("align", "absMiddle");
            functionReturnValue.CssClass = "editorArrow";
            functionReturnValue.Attributes.Add("onMouseOut", "this.className = 'editorArrow'; scrollStop();");
            return functionReturnValue;
        }

        private void SetupMenu() {
            // Calculate innerlayer max width 32 pixel per icon
            int ScrollingLayerWidth = Icons.Count * 26 + extraMenuWidth;

            Table Container = new Table();
            Container.ID = String.Format("{0}_tableContainer", this.ID);
            TableRow tr = new TableRow();
            tr.ID = String.Format("{0}_tableContainerRow", this.ID);
            Container.Rows.Add(tr);

            // // scroll-left image
            TableCell td = new TableCell();
            td.ID = String.Format("{0}_tableContainerLeft", this.ID);
            System.Web.UI.WebControls.Image scrollL = scrollImage();
            scrollL.ImageUrl = _ClientFilesPath + "images/arrawBack.gif";
            scrollL.Attributes.Add("onMouseOver", "this.className = 'editorArrowOver'; scrollR('" + this.ClientID + "_sl','" + this.ClientID + "_slh'," + ScrollingLayerWidth + ");");
            td.Controls.Add(scrollL);
            tr.Cells.Add(td);

            // // Menulayers
            td = new TableCell();
            td.ID = String.Format("{0}_tableContainerButtons", this.ID);

            HtmlGenericControl outerLayer = new HtmlGenericControl();
            outerLayer.TagName = "div";
            outerLayer.ID = this.ID + "_slh";
            outerLayer.Attributes.Add("class", "slh");
            outerLayer.Style.Add("height", "26px");
            string tmp = this.Width.ToString();

            outerLayer.Style.Add("width", (this.Width.Value - 18).ToString() + "px");
            td.Controls.Add(outerLayer);

            HtmlGenericControl menuLayer = new HtmlGenericControl();
            menuLayer.TagName = "div";
            menuLayer.ID = this.ID + "_sl";
            menuLayer.Style.Add("top", "0px");
            menuLayer.Style.Add("left", "0px");
            menuLayer.Attributes.Add("class", "sl");
            menuLayer.Style.Add("height", "26px");
            menuLayer.Style.Add("width", ScrollingLayerWidth + "px");

            HtmlGenericControl nobr = new HtmlGenericControl();
            nobr.TagName = "nobr";
            nobr.ID = String.Format("{0}_nobr", this.ID);
            menuLayer.Controls.Add(nobr);

            // // add all icons to the menu layer
            foreach (Control item in Icons) {
                menuLayer.Controls.Add(item);

                if (item.ID != "") {
                    iconIds = iconIds + item.ID + ",";
                }
            }

            outerLayer.Controls.Add(new LiteralControl("<script>RegisterScrollingMenuButtons('" + this.ClientID + "', '" + iconIds + "');</script>"));

            outerLayer.Controls.Add(menuLayer);

            tr.Cells.Add(td);

            // // scroll-right image
            td = new TableCell();
            td.ID = String.Format("{0}_tableContainerRight", this.ID);
            System.Web.UI.WebControls.Image scrollR = scrollImage();
            scrollR.ImageUrl = _ClientFilesPath + "images/arrowForward.gif";
            scrollR.Attributes.Add("onMouseOver", "this.className = 'editorArrowOver'; scrollL('" + this.ClientID + "_sl','" + this.ClientID + "_slh'," + ScrollingLayerWidth + ");");
            td.Controls.Add(scrollR);
            tr.Cells.Add(td);

            this.Controls.Add(Container);
        }

    }
}