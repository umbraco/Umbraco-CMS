<%@ Control Language="c#" AutoEventWireup="True" Codebehind="LatestEdits.ascx.cs" Inherits="dashboardUtilities.LatestEdits" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<asp:Repeater id="Repeater1" runat="server">
	<HeaderTemplate>
	<p class="guiDialogHeader"><%=umbraco.ui.Text("defaultdialogs", "lastEdited", bp.getUser())%>:</p>
	<span class="guiDialogNormal">
	</HeaderTemplate>
             
	<ItemTemplate>
		<%# PrintNodeName(DataBinder.Eval(Container.DataItem, "NodeId"), DataBinder.Eval(Container.DataItem, "datestamp")) %>
	</ItemTemplate>
	    
	<FooterTemplate>
	</span>
	</FooterTemplate>

</asp:Repeater>
