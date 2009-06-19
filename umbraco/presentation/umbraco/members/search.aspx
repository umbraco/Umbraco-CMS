<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../masterpages/umbracoPage.Master" CodeBehind="search.aspx.cs" Inherits="umbraco.presentation.members.search" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
  <cc1:UmbracoPanel ID="panel1" runat="server">
    <cc1:Pane ID="pane1" runat="server">
    <h3>Member Search</h3>
<p class="guiDialogNormal">
	<asp:TextBox id="searchQuery" runat="server"></asp:TextBox></p>
<asp:PlaceHolder ID="umbExtended" Visible="false" runat="server">
<p class="guiDialogNormal">
	<asp:CheckBox id="CheckBoxExtended" runat="server" Text="Extended search (slow)"></asp:CheckBox>
	<br />
</p>
</asp:PlaceHolder>
	<asp:Button id="ButtonSearch" runat="server" Text="Button" onclick="ButtonSearch_Click"></asp:Button><br />
<asp:Repeater runat="server" Visible="false" ID="searchResult">
<HeaderTemplate><h3>Results:</h3><table cellpadding="5"></HeaderTemplate>
<ItemTemplate>
<tr><td>
<asp:HyperLink NavigateUrl='<%# "editmember.aspx?id=" + DataBinder.Eval(Container.DataItem,"username")%>' Text='<%#DataBinder.Eval(Container.DataItem, "username")%>'  
runat="server" ID="Hyperlink1" NAME="Hyperlink1"/>
</td><td><%#Eval("email") %></td></tr>
</ItemTemplate>
<FooterTemplate></table></FooterTemplate>
</asp:Repeater>

<asp:DataGrid id="umbMember" runat="server" Visible="False" Width="100%">
	<AlternatingItemStyle BackColor="#E0E0E0"></AlternatingItemStyle>
	<HeaderStyle BackColor="Silver"></HeaderStyle>
</asp:DataGrid>

   </cc1:Pane>
  </cc1:UmbracoPanel>
</asp:Content>
