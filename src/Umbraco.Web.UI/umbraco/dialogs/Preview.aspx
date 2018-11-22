<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../masterpages/umbracoDialog.Master" Inherits="umbraco.presentation.dialogs.Preview" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="body" runat="server">

<cc1:Feedback ID="feedback1" runat="server" />

<cc1:Pane ID="pane_form" runat="server">
<cc1:PropertyPanel ID="PropertyPanel1" runat="server" Name="Document">
<asp:Literal ID="docLit" runat="server"></asp:Literal>
</cc1:PropertyPanel>
<cc1:PropertyPanel ID="PropertyPanel2" runat="server" Name="Change Set">
<asp:Literal ID="changeSetUrl" runat="server"></asp:Literal>
</cc1:PropertyPanel>
</cc1:Pane>



 </asp:Content>