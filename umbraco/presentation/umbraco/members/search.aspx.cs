using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Security;
using umbraco.cms.businesslogic.member;
using umbraco.DataLayer.SqlHelpers;
using umbraco.BusinessLogic;

namespace umbraco.presentation.members
{
    public partial class search : BasePages.UmbracoEnsuredPage
    {
        protected override void OnLoad(EventArgs e)
        {
            if (Member.InUmbracoMemberMode())
                umbExtended.Visible = true;

            ButtonSearch.Text = ui.Text("search");
        }

        protected void ButtonSearch_Click(object sender, System.EventArgs e)
        {
            int total;

            if (!Member.InUmbracoMemberMode())
            {

                if (searchQuery.Text.Contains("@"))
                {
                    searchResult.DataSource = Membership.FindUsersByEmail(searchQuery.Text);// , 1, 500, out total);
                }
                else
                {
                    searchResult.DataSource = Membership.FindUsersByName(searchQuery.Text + "%");// , 1, 500, out total);
                }
                searchResult.DataBind();
                searchResult.Visible = true;
            }
            else
            {
                umbMember.Visible = true;
                string sql = "select distinct '<a href=\"editMember.aspx?id=' + convert(nvarchar(10), nodeId) + '\">' +  text + '</a>' as Name, loginName, email from cmsMember inner join umbracoNode on umbracoNode.id = nodeId where text like @query or email like @query or loginName like @query";

                if (CheckBoxExtended.Checked)
                {
                    sql += " or nodeId in (select nodeId from cmsPropertyData inner join cmsMember on nodeId = contentNodeid where dataNvarchar like @query or dataNtext like @query)";
                }
                sql += " order by '<a href=\"editMember.aspx?id=' + convert(nvarchar(10), nodeId) + '\">' +  text + '</a>'";

                umbMember.DataSource = 
                    BusinessLogic.Application.SqlHelper.ExecuteReader(
                        sql,
                        BusinessLogic.Application.SqlHelper.CreateParameter("@query", "%" + searchQuery.Text + "%"));
                umbMember.DataBind();
            }
        }
    }
}
