<%@ Page Title="" Language="C#" MasterPageFile="../masterpages/umbracoPage.Master" AutoEventWireup="true" CodeBehind="details.aspx.cs" Inherits="umbraco.presentation.umbraco.translation.details" %>
<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

<style type="text/css">
  .fieldsTable tr{
    border-color: #D9D7D7 !Important;
  }  
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

<ui:UmbracoPanel ID="panel1" Text="Details" runat="server" hasMenu="false">

<ui:Pane runat="server" ID="pane_details" Text="Translation details">
  <ui:PropertyPanel id="pp_date" runat="server" Text="Task opened"></ui:PropertyPanel>
  <ui:PropertyPanel ID="pp_owner" runat="server" Text="Assigned to you by"></ui:PropertyPanel>
  <ui:PropertyPanel ID="pp_totalWords" runat="server" Text="Total words"></ui:PropertyPanel>
  <ui:PropertyPanel ID="pp_comment" runat="server" Text="Task comment"></ui:PropertyPanel>
</ui:Pane>


<ui:Pane ID="pane_tasks" runat="server" Text="Tasks">
  <ui:PropertyPanel ID="pp_xml" runat="server" Text="Translation xml" />
  <ui:PropertyPanel ID="pp_upload" runat="server" Text="Upload translation"/>
  <ui:PropertyPanel ID="pp_closeTask" runat="server" Text="Close task">
      <asp:Button runat="server" ID="bt_close" Text="Close task" OnClick="closeTask"/>
  </ui:PropertyPanel>
</ui:Pane>


<ui:Pane ID="pane_fields" runat="server" Text="Fields">
  <asp:DataGrid ID="dg_fields" runat="server" GridLines="Horizontal" HeaderStyle-Font-Bold="true" CssClass="fieldsTable" Width="100%" BorderStyle="None" />
</ui:Pane>


</ui:UmbracoPanel>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="footer" runat="server">
</asp:Content>
