<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../masterpages/umbracoPage.Master" CodeBehind="publish.aspx.cs" Inherits="umbraco.presentation.actions.publish" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
  <style type="text/css">
    body{background-image: none !Important;}
  </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">

		  <cc1:UmbracoPanel ID="Panel2" Text="Publish" AutoResize="false" Width="500px" Height="200px" runat="server">
          <asp:Panel ID="confirm" runat="server">
			    <cc1:Pane ID="pane_publish" runat="server">
			    <p>
			      <asp:Literal ID="warning" runat="server"></asp:Literal>
			    </p>
			    </cc1:Pane>      
          <br />			    
			    <p>
			      <asp:Button ID="deleteButton" runat="server" OnClick="deleteButton_Click" />
			    </p>    			
			    </asp:Panel>
			    
			     <cc1:Pane ID="deleteMessage" runat="server" Visible="false">
			      <p><asp:Literal ID="deleted" runat="server"></asp:Literal></p>
			     </cc1:Pane>			
			</cc1:UmbracoPanel>
			
</asp:Content>