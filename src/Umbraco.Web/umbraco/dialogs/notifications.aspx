<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" Codebehind="notifications.aspx.cs" AutoEventWireup="True"
  Inherits="umbraco.dialogs.notifications" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
<style type="text/css">
  .propertyItemheader{width: 190px !Important;}
</style>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">

<cc1:Feedback ID="feedback" runat="server" />

<cc1:Pane ID="pane_form" runat="server">

</cc1:Pane>

<asp:Panel ID="pl_buttons" runat="server">
<br />
<asp:Button ID="Button1" runat="server" Text="" OnClick="Button1_Click"></asp:Button>
&nbsp; <em><%= umbraco.ui.Text("or") %></em> &nbsp;<a href="#" style="color: blue"  onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>      
</asp:Panel>

</asp:Content>