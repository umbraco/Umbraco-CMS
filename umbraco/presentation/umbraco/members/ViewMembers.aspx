<%@ Page Title="" Language="C#" MasterPageFile="~/umbraco/masterpages/umbracoPage.Master"
  AutoEventWireup="true" CodeBehind="ViewMembers.aspx.cs" Inherits="umbraco.presentation.members.ViewMembers" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
  <cc1:UmbracoPanel ID="panel1" runat="server">
    <cc1:Pane ID="pane1" runat="server">
    
    <asp:Repeater ID="rp_members" OnItemDataBound="bindMember" runat="server">
    <HeaderTemplate>
    <table width="100%" rules="rows">
    <thead>
    <tr style="border-bottom: 2px solid #D9D7D7;"><th><%= umbraco.ui.Text("name") %></th><th><%= umbraco.ui.Text("email") %></th><th colspan="2"><%= umbraco.ui.Text("login") %></th></tr>
    </thead>
    <tbody>
    </HeaderTemplate>
      <ItemTemplate>
        <tr style="border-bottom: 1px solid #D9D7D7;">
          <td><asp:Literal ID="lt_name" runat="server"></asp:Literal></td>
          <td><asp:Literal ID="lt_email" runat="server"></asp:Literal></td>
          <td><asp:Literal ID="lt_login" runat="server"></asp:Literal></td>
          <td><asp:Button ID="bt_delete" runat="server" OnCommand="deleteMember" Text="Delete" /></td>
        </tr>
      </ItemTemplate>
    <FooterTemplate>
    </tbody>
    </table>
    </FooterTemplate>
    </asp:Repeater>
    </cc1:Pane>
  </cc1:UmbracoPanel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="footer" runat="server">
</asp:Content>
