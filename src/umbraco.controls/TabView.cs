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
                _tabsHolder.Controls.Add(tabPage);
            }
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

        private string ActiveTabId
        {
            get
            {
                if (this.Parent.Page.IsPostBack)
                {
                    return this.Parent.Page.Request.Form[this.ClientID + "_activetab"];
                }
                return "tab01";
            }
        }
        
        /*
	    protected override void Render(HtmlTextWriter writer)
	    {
	        writer.WriteLine("<div id='" + this.ClientID + "' style='height:" + this.Height.Value + "px;width:" + this.Width.Value + "px;'>");
	        writer.WriteLine("  <div class='header'>");
	        writer.WriteLine("      <ul>");
	        for (int i = 0; i <= _tabs.Count - 1; i++)
	        {
	            string TabPageCaption = (string) _tabs[i];
	            string TabId = this.ClientID + "_tab0" + (i + 1);
	            writer.WriteLine("          <li id='" + TabId + "' class='tabOff'>");
	            writer.WriteLine("              <a id='" + TabId + "a' href='#' onclick=\"setActiveTab('" + this.ClientID + "','" + TabId + "'," + this.ClientID + "_tabs); return false;\">");
	            writer.WriteLine("                  <span><nobr>" + TabPageCaption + "</nobr></span>");
	            writer.WriteLine("              </a>");
	            writer.WriteLine("          </li>");
	        }
	        writer.WriteLine("      </ul>");
	        writer.WriteLine("  </div>");
	        writer.WriteLine("  <div id='' class='tabpagecontainer'>");
	        this.RenderChildren(writer);
	        writer.WriteLine("\t</div>");
	        writer.WriteLine("\t<div class='footer'><div class='status'><h2>" + this._status + "</h2></div></div>");
	        writer.WriteLine("</div>");
	        writer.WriteLine("<input type='hidden' name='" + this.ClientID + "_activetab' id='" + this.ClientID + "_activetab' value='" + this.ActiveTabId + "'/>");
	    }*/
	}
}