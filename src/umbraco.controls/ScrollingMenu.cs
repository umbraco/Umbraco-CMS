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
using umbraco.IO;


namespace umbraco.uicontrols
{

    [ToolboxData("<{0}:ScrollingMenu runat=server></{0}:ScrollingMenu>")]
    public class ScrollingMenu : System.Web.UI.WebControls.PlaceHolder
    {
        public ArrayList Icons = new ArrayList();
        private string iconIds;
        private int extraMenuWidth = 0;

        public MenuIconI NewIcon(int Index)
        {
            MenuIcon Icon = new MenuIcon();
            Icons.Insert(Index, Icon);
            return Icon;
        }

        public MenuIconI NewIcon()
        {
            MenuIcon icon = new MenuIcon();
            Icons.Add(icon);
            return icon;
        }

        internal MenuSplitButton NewSplitButton()
        {
            var menu = new MenuSplitButton();
            Icons.Add(menu);
            return menu;
        }

        public MenuButton NewButton(int index = -1)
        {
            MenuButton btn = new MenuButton();

            if (index > -1)
                Icons.Insert(index, btn);
            else
                Icons.Add(btn);

            return btn;
        }

        public MenuImageButton NewImageButton()
        {
            MenuImageButton icon = new MenuImageButton();
            Icons.Add(icon);
            return icon;
        }

        public MenuImageButton NewImageButton(int Index)
        {
            MenuImageButton icon = new MenuImageButton();
            Icons.Insert(Index, icon);
            return icon;
        }


        public System.Web.UI.WebControls.DropDownList NewDropDownList()
        {
            DropDownList Icon = new DropDownList();
            Icons.Add(Icon);
            return Icon;
        }

        public void NewElement(string ElementName, string ElementId, string ElementClass, int ExtraWidth)
        {
            Icons.Add(new LiteralControl("<" + ElementName + " class=\"" + ElementClass + "\" id=\"" + ElementId + "\"></" + ElementName + ">"));
            extraMenuWidth = extraMenuWidth + ExtraWidth;
        }

        public void InsertSplitter()
        {
            Splitter icon = new Splitter();
            Icons.Add(icon);
        }
        public void InsertSplitter(int Index)
        {
            Splitter icon = new Splitter();
            Icons.Insert(Index, icon);
        }

        /// <summary>
        /// Inserts a new web control into the scrolling menu
        /// </summary>
        /// <param name="control"></param>
        /// <param name="extraWidth">The additional width to extend the scrolling menu by if the control being inserted is wider than the standard</param>
        public void InsertNewControl(Control control, int extraWidth = 0)
        {
           Icons.Add(control);
           // _extraMenuWidth = _extraMenuWidth + extraWidth;
        }


        /// <summary>
        /// Finds the index of the n-th Splitter in this Menu
        /// </summary>
        /// <param name="n"></param>
        /// <returns>The index of the n-th Splitter, or -1 if the number of Splitters is smaller than n</returns>
        public int FindSplitter(int n)
        {
            var count = 0;
            for (var i = 0; i < Icons.Count; i++)
            {
                if (Icons[i].GetType() == typeof(Splitter))
                {
                    count++;

                    if (count == n)
                        return i;
                }
            }

            return -1;
        }


        private HtmlGenericControl toolbar;
        private HtmlGenericControl wrap;
        private HtmlGenericControl group;

        protected override void CreateChildControls()
        {   
            toolbar = new HtmlGenericControl { TagName = "div" };
            toolbar.Attributes.Add("class", "btn-toolbar umb-btn-toolbar");
            this.Controls.Add(toolbar);

            group = new HtmlGenericControl { TagName = "div", ID = this.ClientID + "_group" };
            group.Attributes.Add("class", "btn-group");
            toolbar.Controls.Add(group);
        }

        protected override void OnLoad(EventArgs e)
        {
            foreach (Control item in Icons)
            {
                group.Controls.Add(item);
                if (item.ID != "")
                    iconIds = iconIds + item.ID + ",";
            }
            base.OnLoad(e);
        }
        
       

        protected override void OnInit(EventArgs e)
        {
            
            EnsureChildControls();
            base.OnInit(e);
        }

    }
}