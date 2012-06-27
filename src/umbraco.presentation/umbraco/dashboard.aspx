<%@ Page language="c#" MasterPageFile="masterpages/umbracoPage.Master" Title="dashboard" Codebehind="dashboard.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.dashboard" trace="false" validateRequest="false"%>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="body" ID="ContentBody" runat="server">
			<cc1:UmbracoPanel id="Panel2" runat="server" Height="224px" Width="412px" hasMenu="false">
				<div style="padding: 2px 15px 0px 15px">
				<asp:PlaceHolder id="dashBoardContent" Runat="server"></asp:PlaceHolder>
				</div>
			</cc1:UmbracoPanel>
			<cc1:TabView id="dashboardTabs" Width="400px" Visible="false" runat="server" />
</asp:Content>