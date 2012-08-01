<%@ Page Language="C#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="true" Codebehind="sendToTranslation.aspx.cs" Inherits="umbraco.presentation.dialogs.sendToTranslation" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<style type="text/css">
  .propertyItemheader{width: 160px !Important;}
</style>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">

<cc1:Feedback ID="feedback" runat="server" />

<cc1:Pane ID="pane_form" runat="server">
  <cc1:PropertyPanel ID="pp_translator" runat="server">
    <asp:DropDownList ID="translator" runat="server"></asp:DropDownList>
  </cc1:PropertyPanel>
  <cc1:PropertyPanel ID="pp_language" runat="server">
    <asp:DropDownList ID="language" runat="server"></asp:DropDownList>
    <asp:Literal ID="defaultLanguage" Visible="false" runat="server"></asp:Literal>
  </cc1:PropertyPanel>
  <cc1:PropertyPanel ID="pp_includeSubs" runat="server">
    <asp:CheckBox ID="includeSubpages" runat="server" />
  </cc1:PropertyPanel>
  <cc1:PropertyPanel ID="pp_comment" runat="server">
    <asp:TextBox TextMode="multiLine" runat="Server" Rows="4" ID="comment"></asp:TextBox>
  </cc1:PropertyPanel>
</cc1:Pane>

<asp:Panel ID="pl_buttons" runat="server">
<p>
<asp:Button ID="doTranslation" runat="Server" OnClick="doTranslation_Click" />
&nbsp; <em><%= umbraco.ui.Text("or") %></em> &nbsp;<a href="#" onClick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
</p>
</asp:Panel>
</asp:Content>
      