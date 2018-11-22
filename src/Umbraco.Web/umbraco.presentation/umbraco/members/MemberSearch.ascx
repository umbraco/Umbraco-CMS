<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="MemberSearch.ascx.cs" Inherits="umbraco.presentation.umbraco.members.MemberSearch" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
  <div class="dashboardWrapper">
    <h2>Member Search</h2>
    <img src="<%= Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) + "/dashboard/images/membersearch.png" %>" alt="Member Search" class="dashboardIcon" />
<p class="guiDialogNormal">
	<asp:TextBox id="searchQuery" runat="server"></asp:TextBox>
	<asp:Button id="ButtonSearch" runat="server" Text="Button" onclick="ButtonSearch_Click"></asp:Button></p>

	<cc1:Pane ID="resultsPane" runat="server" Visible="false">
	
	<asp:Repeater ID="rp_members" runat="server">
	<HeaderTemplate>
	<table rules="rows" border="0" class="members_table">
	<thead>
	<tr><th><%= umbraco.ui.Text("name") %></th><th><%= umbraco.ui.Text("email") %></th><th><%= umbraco.ui.Text("login") %></th></tr>
	</thead>
	<tbody>
	</HeaderTemplate>
	  <ItemTemplate>
		<tr>
		  <td><asp:HyperLink runat="server" NavigateUrl='<%# Umbraco.Core.IO.SystemDirectories.Umbraco + "/members/EditMember.aspx?id=" + Eval("Id") %>'><%# Eval("Name") %></asp:HyperLink></td>
		  <td><%# Eval("Email") %></td>
		  <td><%# Eval("LoginName") %></td>
		</tr>
	  </ItemTemplate>
	  <AlternatingItemTemplate>
		<tr class="alt">
		  <td><asp:HyperLink runat="server" NavigateUrl='<%# Umbraco.Core.IO.SystemDirectories.Umbraco + "/members/EditMember.aspx?id=" + Eval("Id") %>'><%# Eval("Name") %></asp:HyperLink></td>
		  <td><%# Eval("Email") %></td>
		  <td><%# Eval("LoginName") %></td>
		</tr>
		</AlternatingItemTemplate>
	<FooterTemplate>
	</tbody>
	</table>
	</FooterTemplate>
	</asp:Repeater>
       </cc1:Pane>
       </div>
