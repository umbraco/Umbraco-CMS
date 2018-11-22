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
using umbraco.BusinessLogic;

namespace umbraco.uicontrols
{
    public class UmbracoPanel : Panel
    {
        private ScrollingMenu _menu = new ScrollingMenu();
        public UmbracoPanel()
        {

        }
        public UmbracoPanel(object input)
        {

        }

        private bool _hasMenu = false;
        private string _StatusBarText = "";
        private string _text;
        private bool _autoResize = true;

        public bool hasMenu
        {
            get { return _hasMenu; }
            set { _hasMenu = value; }
        }

        public bool AutoResize
        {
            get { return _autoResize; }
            set { _autoResize = value; }
        }

        public string Text
        {
            get
            {
                if (_text == "")
                    _text = "&nbsp;";

                return _text;
            }
            set { _text = value; }
        }

        public string StatusBarText
        {
            get { return _StatusBarText; }
            set { _StatusBarText = value; }
        }

        public ScrollingMenu Menu
        {
            get { return _menu; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

   
        internal HtmlGenericControl header = new HtmlGenericControl();
        internal HtmlGenericControl row = new HtmlGenericControl();
        internal HtmlGenericControl leftcol = new HtmlGenericControl();
        internal HtmlGenericControl rightcol = new HtmlGenericControl();
        internal HtmlGenericControl title = new HtmlGenericControl();

        internal HtmlGenericControl body = new HtmlGenericControl();

        protected override void CreateChildControls()
        {
            //fucking webforms controls and their rendering..
            CssClass = "umb-panel form-horizontal umb-panel-nobody";
           ID = base.ClientID + "_container";

            header.TagName = "div";
            header.ID = base.ClientID + "_header";
            header.Attributes.Add("class","umb-panel-header");
            
            row.TagName = "div";
            row.Attributes.Add("class", "row-fluid");
            header.Controls.Add(row);

            leftcol.TagName = "span";
            leftcol.Attributes.Add("class", "span8");

            title.TagName = "h1";
            title.Attributes.Add("class", "headline");
            leftcol.Controls.Add(title);
            
            row.Controls.Add(leftcol);

            rightcol.TagName = "span";
            rightcol.Attributes.Add("class", "span4");
            rightcol.Controls.Add(Menu);
            row.Controls.Add(rightcol);

            body.TagName = "div";
            body.Attributes.Add("class", "umb-panel-body row-fluid");

            Width = Unit.Empty;
            Height = Unit.Empty;

            Controls.AddAt(0,header);
            

            base.CreateChildControls();
        }
        

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            title.InnerHtml = Text;
        }
        
    }
}