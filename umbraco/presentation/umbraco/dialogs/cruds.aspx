<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" Codebehind="cruds.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.cruds" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
<style>
  .guiDialogTinyMark{font-size: 9px !Important;}
</style>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">

<cc1:Feedback ID="feedback1" runat="server" />

<cc1:Pane ID="pane_form" runat="server">
<cc1:PropertyPanel runat="server">
 <asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
</cc1:PropertyPanel>
</cc1:Pane>

<asp:Panel ID="panel_buttons" runat="server" Visible="True">
<br />
 <asp:Button ID="Button1" runat="server" Text="" OnClick="Button1_Click"></asp:Button>
 &nbsp; <em>or </em>&nbsp; <a href="#" style="color: blue" onclick="UmbClientMgr.closeModalWindow()">
 <%=umbraco.ui.Text("general", "cancel", this.getUser())%>
 </a>
</asp:Panel>


 </asp:Content>