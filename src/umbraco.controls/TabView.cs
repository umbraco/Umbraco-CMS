using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core;

namespace umbraco.uicontrols {

	[ClientDependency(ClientDependencyType.Javascript, "tabview/javascript.js", "UmbracoClient")]
	[ClientDependency(ClientDependencyType.Css, "tabview/style.css", "UmbracoClient")]
	[ClientDependency(0, ClientDependencyType.Javascript, "Application/NamespaceManager.js", "UmbracoClient")]
	public class TabView : WebControl
	{
	    private readonly ArrayList _tabs = new ArrayList();
		protected ArrayList Panels = new ArrayList();
		private string _status = "";

	    public ArrayList GetPanels()
	    {
	        return Panels;
	    }

	    public TabPage NewTabPage(string text)
	    {
	        _tabs.Add(text);
	        var tp = new TabPage();
	        tp.Width = this.Width;
	        tp.ID = this.ID + "_tab0" + (Panels.Count + 1) + "layer";
	        Panels.Add(tp);
	        this.Controls.Add(tp);
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
	            return this.ClientID + "_tab01";
	        }
	    }

	    protected override void OnPreRender(EventArgs e)
	    {
	        base.OnPreRender(e);
	        SetupClientScript();
	    }

	    private void SetupClientScript()
	    {
	        string strTmp = "";
	        for (int i = 1; i <= _tabs.Count; i++)
	        {
	            if (i > 1)
	                strTmp += ",";
	            strTmp += "\"" + this.ClientID + "_tab0" + i + "\"";
	        }
	        this.Page.ClientScript.RegisterStartupScript(
	            this.GetType(),
	            this.ClientID + "TabCollection", ";var " + this.ClientID + "_tabs = new Array(" + strTmp + ");setActiveTab('" + this.ClientID + "','" + this.ActiveTabId + "'," + this.ClientID + "_tabs);",
	            true);

	        if (_autoResize)
	            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "TabviewEvents", "jQuery(document).ready(function(){resizeTabView(" + this.ClientID + "_tabs, '" + this.ClientID + "'); }); jQuery(window).resize(function(){ resizeTabView(" + this.ClientID + "_tabs, '" + this.ClientID + "'); });", true);
	    }

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
	            writer.WriteLine("                  <span><nobr>" + HttpUtility.HtmlEncode(TabPageCaption) + "</nobr></span>");
	            writer.WriteLine("              </a>");
	            writer.WriteLine("          </li>");
	        }
	        writer.WriteLine("      </ul>");
	        writer.WriteLine("  </div>");
	        writer.WriteLine("  <div id='' class='tabpagecontainer'>");
	        this.RenderChildren(writer);
	        writer.WriteLine("\t</div>");
	        writer.WriteLine("\t<div class='footer'><div class='status'><h2>" + HttpUtility.HtmlEncode(this._status) + "</h2></div></div>");
	        writer.WriteLine("</div>");
            writer.WriteLine("<input type=\"hidden\" name=\"" + this.ClientID + "_activetab\" id=\"" + this.ClientID + "_activetab\" value=\"" + HttpUtility.HtmlAttributeEncode(this.ActiveTabId) + "\"/>");
	    }
	}
}