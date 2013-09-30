using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using System.Web.UI.HtmlControls;

namespace umbraco.uicontrols {

	public class TabView : umbraco.uicontrols.UmbracoPanel
	{
	    public readonly ArrayList Tabs = new ArrayList();
		protected ArrayList Panels = new ArrayList();
        protected Dictionary<string, TabPage> TabPages = new Dictionary<string, TabPage>();

		private string _status = "";

        private HtmlGenericControl _tabList = new HtmlGenericControl();
        private HtmlGenericControl _body = new HtmlGenericControl();
        private HtmlGenericControl _tabsHolder = new HtmlGenericControl();

        private HiddenField _activeTabHolder = new HiddenField();

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _tabList.TagName = "ul";
            _tabList.Attributes.Add("class", "nav nav-tabs umb-nav-tabs span12");
            base.row.Controls.Add(_tabList);

            _body.TagName = "div";
            _body.Attributes.Add("class", "umb-panel-body umb-scrollable row-fluid");
            base.Controls.Add(_body);

            _tabsHolder.TagName = "div";
            _tabsHolder.Attributes.Add("class", "tab-content form-horizontal umb-tab-content");
            _tabsHolder.ID = this.ID + "_content";
            _body.Controls.Add(_tabsHolder);
            
            for (int i = 0; i < Tabs.Count; i++)
            {
                var tabPage = TabPages.ElementAt(i).Value;
                tabPage.Active = false;

                if (tabPage.ID == ActiveTabId)
                    tabPage.Active = true;

                _tabsHolder.Controls.Add(tabPage);
            }

            _activeTabHolder.ID = "activeTabHolder";
            _body.Controls.Add(_activeTabHolder);

        }

        protected override void OnInit(EventArgs e)
        {   
            base.OnInit(e);
            base.CssClass = "umb-panel tabbable";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (string.IsNullOrEmpty(Text))
                Text = " ";

            
            for (int i = 0; i < Tabs.Count; i++)
            {
                var tabPage = TabPages.ElementAt(i).Value;
                string tabPageCaption = tabPage.Text;
                string tabId = tabPage.ID;
                
                HtmlGenericControl li = new HtmlGenericControl();
                li.TagName = "li";
                if (tabId == ActiveTabId)
                    li.Attributes.Add("class", "active");
                _tabList.Controls.Add(li);

                HtmlGenericControl a = new HtmlGenericControl();
                a.TagName = "a";
                a.Attributes.Add("href", "#"+tabId);
                a.Attributes.Add("onclick", "$('#" + _activeTabHolder.ClientID + "').val('" + tabId + "');");
                a.Attributes.Add("data-toggle", "tab");
                a.InnerText = tabPageCaption;
                li.Controls.Add(a);
            }
        }

	    public ArrayList GetPanels()
	    {
	        return Panels;
	    }

        public TabPage NewTabPage(string text)
        {
            Tabs.Add(text);
            TabPage tp = new TabPage();
            tp.Width = this.Width;
            tp.ID = "tab0" + (TabPages.Count + 1);
            tp.Text = text;
            tp.parent = this;

            Panels.Add(tp);
            TabPages.Add(tp.ID, tp);

            _tabsHolder.Controls.Add(tp);
            return tp;
        }


	    public string Status
	    {
	        get { return _status; }
	        set { _status = value; }
	    }

	    private bool _autoResize = true;
        public bool AutoResize
	    {
	        get { return _autoResize; }
	        set { _autoResize = value; }
	    }

        public string ActiveTabId
        {
            get
            {

                if (_activeTabHolder.Value != "")
                    return _activeTabHolder.Value;

                if (TabPages.Count > 0)
                    return TabPages.ElementAt(0).Value.ID;

                

                return "tab01";
            }
        }
    }
}