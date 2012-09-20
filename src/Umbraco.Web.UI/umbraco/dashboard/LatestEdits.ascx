<%@ Control Language="c#" AutoEventWireup="True" Codebehind="LatestEdits.ascx.cs" Inherits="dashboardUtilities.LatestEdits" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<umb:CssInclude runat="server" FilePath="propertypane/style.css" PathNameAlias="UmbracoClient" />

<div class="dashboardWrapper">
	<h2><%=umbraco.ui.Text("defaultdialogs", "lastEdited")%></h2>
	<img src="./dashboard/images/logo32x32.png" alt="Umbraco" class="dashboardIcon" />
	<asp:Repeater id="Repeater1" runat="server">
		<ItemTemplate>
			<%# PrintNodeName(DataBinder.Eval(Container.DataItem, "NodeId"), DataBinder.Eval(Container.DataItem, "datestamp")) %>
		</ItemTemplate>
	</asp:Repeater>
</div>
