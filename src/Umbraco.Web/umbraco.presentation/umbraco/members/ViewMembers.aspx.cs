using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.Security;

namespace umbraco.presentation.members
{
    public partial class ViewMembers : BasePages.UmbracoEnsuredPage
    {

        public ViewMembers()
        {
            CurrentApp = BusinessLogic.DefaultApps.member.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            panel1.Text = ui.Text("member");
            BindRp();
        }

        private void BindRp()
        {
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
            string letter = Request.QueryString["letter"];
            if (string.IsNullOrEmpty(letter) == false)
            {
                if (provider.IsUmbracoMembershipProvider())
                {
                    if (letter == "#")
                    {
                        rp_members.DataSource = cms.businesslogic.member.Member.getAllOtherMembers();
                    }
                    else
                    {
                        rp_members.DataSource = cms.businesslogic.member.Member.getMemberFromFirstLetter(letter.ToCharArray()[0]);
                    }
                }
                else
                {
                    rp_members.DataSource = provider.FindUsersByName(letter + "%");
                }
                rp_members.DataBind();
            }
        }

        public void bindMember(object sender, RepeaterItemEventArgs e)
        {
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                if (provider.IsUmbracoMembershipProvider())
                {
                    cms.businesslogic.member.Member mem = (cms.businesslogic.member.Member)e.Item.DataItem;
                    Literal _name = (Literal)e.Item.FindControl("lt_name");
                    Literal _email = (Literal)e.Item.FindControl("lt_email");
                    Literal _login = (Literal)e.Item.FindControl("lt_login");
                    Button _button = (Button)e.Item.FindControl("bt_delete");

                    _name.Text = "<a href='editMember.aspx?id=" + mem.Id.ToString() + "'>" + mem.Text + "</a>";
                    _login.Text = mem.LoginName;
                    _email.Text = mem.Email;

                    _button.CommandArgument = mem.Id.ToString();
                    _button.OnClientClick = "return confirm(\"" + ui.Text("confirmdelete") + "'" + mem.Text + "' ?\")";
                    _button.Text = ui.Text("delete");
                }
                else
                {
                    var mem = (MembershipUser)e.Item.DataItem;
                    Literal _name = (Literal)e.Item.FindControl("lt_name");
                    Literal _email = (Literal)e.Item.FindControl("lt_email");
                    Literal _login = (Literal)e.Item.FindControl("lt_login");
                    Button _button = (Button)e.Item.FindControl("bt_delete");

                    _name.Text = "<a href='editMember.aspx?id=" + mem.UserName + "'>" + mem.UserName + "</a>";
                    _login.Text = mem.UserName;
                    _email.Text = mem.Email;
                    _button.Visible = false;

                }
            }
        }

        public void deleteMember(object sender, CommandEventArgs e)
        {
            int memid = 0;

            if (int.TryParse(e.CommandArgument.ToString(), out memid))
            {
                cms.businesslogic.member.Member mem = new global::umbraco.cms.businesslogic.member.Member(memid);

                if (mem != null)
                    mem.delete();


                BindRp();
            }
        }
    }
}
