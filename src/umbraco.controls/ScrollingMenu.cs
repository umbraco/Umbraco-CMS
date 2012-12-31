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
using Umbraco.Core.IO;


namespace umbraco.uicontrols {

    [ToolboxData("<{0}:ScrollingMenu runat=server></{0}:ScrollingMenu>")]    
	[ClientDependency(ClientDependencyType.Javascript, "scrollingmenu/javascript.js", "UmbracoClient")]
	[ClientDependency(ClientDependencyType.Css, "scrollingmenu/style.css", "UmbracoClient")]
	public class ScrollingMenu : System.Web.UI.WebControls.WebControl 
	{
        private readonly ArrayList _icons = new ArrayList();
        private string _iconIds;
        private int _extraMenuWidth = 0;
        private readonly string _clientFilesPath = IOHelper.ResolveUrl( SystemDirectories.UmbracoClient) + "/scrollingmenu/";

        public MenuIconI NewIcon(int index) 
		{
            MenuIcon icon = new MenuIcon();
            _icons.Insert(index, icon);
            return icon;
        }

        public MenuIconI NewIcon() 
		{
            MenuIcon icon = new MenuIcon();
            _icons.Add(icon);
            return icon;
        }

        public MenuImageButton NewImageButton() 
		{
            MenuImageButton icon = new MenuImageButton();
            _icons.Add(icon);
            return icon;
        }

        public MenuImageButton NewImageButton(int index) 
		{
            MenuImageButton icon = new MenuImageButton();
            _icons.Insert(index, icon);
            return icon;
        }

        public DropDownList NewDropDownList() 
		{
            DropDownList icon = new DropDownList();
            _icons.Add(icon);
            return icon;
        }
        
		public void NewElement(string elementName, string elementId, string elementClass, int extraWidth) 
		{
            _icons.Add(new LiteralControl("<" + elementName + " class=\"" + elementClass + "\" id=\"" + elementId + "\"></" + elementName + ">"));
            _extraMenuWidth = _extraMenuWidth + extraWidth;
        }

	    /// <summary>
	    /// Inserts a new web control into the scrolling menu
	    /// </summary>
	    /// <param name="control"></param>
	    /// <param name="extraWidth">The additional width to extend the scrolling menu by if the control being inserted is wider than the standard</param>
	    public void InsertNewControl(Control control, int extraWidth = 0)
		{
			_icons.Add(control);
			_extraMenuWidth = _extraMenuWidth + extraWidth;
		}

        public void InsertSplitter() 
		{
            Splitter icon = new Splitter();
            _icons.Add(icon);
        }
        public void InsertSplitter(int index) 
		{
            Splitter icon = new Splitter();
            _icons.Insert(index, icon);
        }

        /// <summary>
        /// Finds the index of the n-th Splitter in this Menu
        /// </summary>
        /// <param name="n"></param>
        /// <returns>The index of the n-th Splitter, or -1 if the number of Splitters is smaller than n</returns>
        public int FindSplitter(int n)
        {
            var count = 0;
            for(var i=0; i<_icons.Count; i++)
            {
               if (_icons[i].GetType() == typeof(Splitter))
               {
                   count++;

                   if (count == n)
                       return i;
               }
            }

            return -1;
        }

        protected override void OnLoad(EventArgs e) 
		{
			base.OnLoad(e);
            if (base.Visible)
            {
                SetupMenu();                
            }
        }

        private static System.Web.UI.WebControls.Image ScrollImage() 
		{
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

        private void SetupMenu() 
		{
            // Calculate innerlayer max width 32 pixel per icon
            var scrollingLayerWidth = _icons.Count * 26 + _extraMenuWidth;

            Table container = new Table();
            container.ID = String.Format("{0}_tableContainer", this.ID);
            TableRow tr = new TableRow();
            tr.ID = String.Format("{0}_tableContainerRow", this.ID);
            container.Rows.Add(tr);

            // // scroll-left image
            TableCell td = new TableCell();
            td.ID = String.Format("{0}_tableContainerLeft", this.ID);
            System.Web.UI.WebControls.Image scrollL = ScrollImage();
            scrollL.ImageUrl = _clientFilesPath + "images/arrawBack.gif";
            scrollL.Attributes.Add("onMouseOver", "this.className = 'editorArrowOver'; scrollR('" + this.ClientID + "_sl','" + this.ClientID + "_slh'," + scrollingLayerWidth + ");");
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
            menuLayer.Style.Add("width", scrollingLayerWidth + "px");

            HtmlGenericControl nobr = new HtmlGenericControl();
            nobr.TagName = "nobr";
            nobr.ID = String.Format("{0}_nobr", this.ID);
            menuLayer.Controls.Add(nobr);

            // // add all icons to the menu layer
            foreach (Control item in _icons) {
                menuLayer.Controls.Add(item);

                if (item.ID != "") {
                    _iconIds = _iconIds + item.ID + ",";
                }
            }

            outerLayer.Controls.Add(new LiteralControl("<script>RegisterScrollingMenuButtons('" + this.ClientID + "', '" + _iconIds + "');</script>"));

            outerLayer.Controls.Add(menuLayer);

            tr.Cells.Add(td);

            // // scroll-right image
            td = new TableCell();
            td.ID = String.Format("{0}_tableContainerRight", this.ID);
            System.Web.UI.WebControls.Image scrollR = ScrollImage();
            scrollR.ImageUrl = _clientFilesPath + "images/arrowForward.gif";
            scrollR.Attributes.Add("onMouseOver", "this.className = 'editorArrowOver'; scrollL('" + this.ClientID + "_sl','" + this.ClientID + "_slh'," + scrollingLayerWidth + ");");
            td.Controls.Add(scrollR);
            tr.Cells.Add(td);

            this.Controls.Add(container);
        }

    }
}