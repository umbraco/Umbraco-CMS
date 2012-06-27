<%@ Control Language="c#" AutoEventWireup="True" Codebehind="LatestEdits.ascx.cs" Inherits="dashboardUtilities.LatestEdits" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<link href="/umbraco_client/propertypane/style.css" rel="stylesheet" />

<asp:Repeater id="Repeater1" runat="server">
	<HeaderTemplate>
    <div class="propertypane">
        <div class="guiDialogNormal" style="margin: 10px">
        <h2><%=umbraco.ui.Text("defaultdialogs", "lastEdited")%>:</h2>
	</HeaderTemplate>
             
	<ItemTemplate>
		<%# PrintNodeName(DataBinder.Eval(Container.DataItem, "NodeId"), DataBinder.Eval(Container.DataItem, "datestamp")) %>
	</ItemTemplate>
	    
	<FooterTemplate>
	    </div>
    </div>
	</FooterTemplate>

</asp:Repeater>
