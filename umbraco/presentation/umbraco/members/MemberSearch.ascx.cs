using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.member;
using System.Web.Security;

namespace umbraco.presentation.umbraco.members
{
    public partial class MemberSearch : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Member.InUmbracoMemberMode())

                ButtonSearch.Text = ui.Text("search");
        }

        protected void ButtonSearch_Click(object sender, System.EventArgs e)
        {
            resultsPane.Visible = true;

            if (!Member.InUmbracoMemberMode())
            {

                IEnumerable<MemberSearchResult> results;
                if (searchQuery.Text.Contains("@"))
                {
                    results = from MembershipUser x in Membership.FindUsersByEmail(searchQuery.Text)
                              select new MemberSearchResult() { Id = x.UserName, Email = x.Email, LoginName = x.UserName, Name = x.UserName };
                }
                else
                {
                    results = from MembershipUser x in Membership.FindUsersByName(searchQuery.Text + "%")
                              select new MemberSearchResult() { Id = x.UserName, Email = x.Email, LoginName = x.UserName, Name = x.UserName };
                }

                rp_members.DataSource = results;
                rp_members.DataBind();
            }
            else
            {

                string query = searchQuery.Text.ToLower();
                var internalSearcher = UmbracoContext.Current.InternalMemberSearchProvider;

                IEnumerable<MemberSearchResult> results;
                if (!String.IsNullOrEmpty(query))
                {

                    results = internalSearcher.Search(query, false).Select(x => new MemberSearchResult()
                    {
                        Id = x["id"],
                        Name = x["nodeName"],
                        Email = x["email"],
                        LoginName = x["loginName"]
                    });
                    rp_members.DataSource = results;
                    rp_members.DataBind();
                }
                else
                {
                    resultsPane.Visible = false;
                }
            }
        }
        public class MemberSearchResult
        {
            public string Id { get; set; }
            public string LoginName { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }
    }
}