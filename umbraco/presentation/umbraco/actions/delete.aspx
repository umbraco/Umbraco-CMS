<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../masterpages/umbracoPage.Master" CodeBehind="delete.aspx.cs" Inherits="umbraco.presentation.actions.delete" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
  <style type="text/css">
    body{background-image: none !Important;}
  </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
      <cc1:UmbracoPanel ID="Panel2" runat="server" AutoResize="false" Width="500px" Height="200px"  Text="Delete">
      
     
      <asp:Panel ID="confirm" runat="server">
		    <cc1:Pane ID="pane_delete" runat="server">
			  <p><asp:Literal ID="warning" runat="server"></asp:Literal></p>
			  </cc1:Pane>
			  <p>
			  <asp:Button ID="deleteButton" runat="server" OnClick="deleteButton_Click" />
			  </p>
			</asp:Panel>
			
			
			<cc1:Pane ID="deleteMessage" runat="server" Visible="false">
			  <p><asp:Literal ID="deleted" runat="server"></asp:Literal></p>
			</cc1:Pane>
			
			
			</cc1:UmbracoPanel>
</asp:Content>
