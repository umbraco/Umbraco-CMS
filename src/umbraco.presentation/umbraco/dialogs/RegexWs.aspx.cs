using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace umbraco.presentation.dialogs {
    public partial class RegexWs : BasePages.UmbracoEnsuredPage {
        private DataSet ds = new DataSet();

        public RegexWs()
        {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();

        }

        protected void Page_Load(object sender, EventArgs e) {
            pp_search.Text = ui.Text("general", "search");
            bt_search.Text = ui.Text("general", "search");
        }

        protected void findRegex(object sender, EventArgs e) {
            regexPanel.Visible = true;

            try {

                ds.Tables.Clear();

                webservices.RegexComWebservice regexLib = new global::umbraco.presentation.webservices.RegexComWebservice();
                ds = regexLib.listRegExp(searchField.Text, "", 0, 10);
                
                results.DataSource = ds;
                results.DataBind();
                
                regexLib.Dispose();
                ds.Clear();
                ds.Dispose();
            } catch{
                Literal err = new Literal();
                err.Text = "<div class='diff'><p>" + ui.Text("defaultdialogs", "regexSearchError") + "</p></div>";
                regexPanel.Controls.Clear();
                regexPanel.Controls.Add(err);
            }
        }

        protected void onRegexBind(object sender, RepeaterItemEventArgs e) {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) {
                DataRowView drw = (DataRowView)e.Item.DataItem;

                Literal _header = (Literal)e.Item.FindControl("header");
                Literal _desc = (Literal)e.Item.FindControl("desc");
                Literal _regex = (Literal)e.Item.FindControl("regex");
               
                _header.Text = "<a href=\"javascript:chooseRegex('" + drw["expression"].ToString().Replace("\\","\\\\") + "');\">" +  drw["Title"].ToString() + "</a>";
                _desc.Text = drw["description"].ToString();
                _regex.Text = drw["expression"].ToString().Trim();
               
            }
        }

    }
}
