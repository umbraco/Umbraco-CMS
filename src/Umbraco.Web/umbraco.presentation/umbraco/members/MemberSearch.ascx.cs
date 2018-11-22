using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using umbraco.cms.businesslogic.member;
using System.Web.Security;
using Umbraco.Core.Security;

namespace umbraco.presentation.umbraco.members
{
    public partial class MemberSearch : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

            if (provider.IsUmbracoMembershipProvider())

                ButtonSearch.Text = ui.Text("search");
        }

        protected void ButtonSearch_Click(object sender, EventArgs e)
        {
            resultsPane.Visible = true;
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
            if (provider.IsUmbracoMembershipProvider())
            {
                var query = searchQuery.Text.ToLower();
                var internalSearcher = UmbracoContext.Current.InternalMemberSearchProvider;

                if (String.IsNullOrEmpty(query) == false)
                {
                    var criteria = internalSearcher.CreateSearchCriteria("member", BooleanOperation.Or);
                    var fields = new[] {"id", "__nodeName", "email"};
                    var term = new[] {query.ToLower().Escape()};
                    var operation = criteria.GroupedOr(fields, term).Compile();

                    var results = internalSearcher.Search(operation)
                        .Select(x => new MemberSearchResult
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
            else
            {
                IEnumerable<MemberSearchResult> results;
                if (searchQuery.Text.Contains("@"))
                {
                    results = from MembershipUser x in provider.FindUsersByEmail(searchQuery.Text)
                        select
                            new MemberSearchResult()
                            {
                                Id = x.UserName,
                                Email = x.Email,
                                LoginName = x.UserName,
                                Name = x.UserName
                            };
                }
                else
                {
                    results = from MembershipUser x in provider.FindUsersByName(searchQuery.Text + "%")
                        select
                            new MemberSearchResult()
                            {
                                Id = x.UserName,
                                Email = x.Email,
                                LoginName = x.UserName,
                                Name = x.UserName
                            };
                }

                rp_members.DataSource = results;
                rp_members.DataBind();
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