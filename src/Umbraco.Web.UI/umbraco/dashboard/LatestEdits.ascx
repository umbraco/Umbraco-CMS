<%@ Control Language="c#" AutoEventWireup="True" Codebehind="LatestEdits.ascx.cs" Inherits="dashboardUtilities.LatestEdits" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<h3><%=umbraco.ui.Text("defaultdialogs", "lastEdited")%></h3>
<ul class="nav nav-stacked">
<asp:Repeater id="Repeater1" runat="server">
	<ItemTemplate>
		<li><%# PrintNodeName(DataBinder.Eval(Container.DataItem, "NodeId"), DataBinder.Eval(Container.DataItem, "datestamp")) %></li>
	</ItemTemplate>
</asp:Repeater>
</ul>