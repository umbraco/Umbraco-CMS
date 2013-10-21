<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NodePermissions.ascx.cs" Inherits="umbraco.cms.presentation.user.NodePermissions" %>
<p class="umb-abstract">
    <%=umbraco.ui.Text("user", "permissionSelectedPages")%>
	<br/>
	<asp:Literal runat="server" ID="lt_names" />
</p>

<asp:Panel ID="pnlReplaceChildren" runat="server">
	<p>
		<input type="checkbox" name="chkChildPermissions" id="chkChildPermissions" />
		<strong>
			<label for="chkChildPermissions" class="checkbox inline">
				<%= umbraco.ui.Text("user", "permissionReplaceChildren")%>
			</label>
		</strong>
	</p>
</asp:Panel>

<asp:Label runat="server" ID="lblMessage" />
<asp:Repeater runat="server" ID="rptPermissionsList">
	<HeaderTemplate>
		<ul id="nodepermissionsList">
	</HeaderTemplate>
	<ItemTemplate>
		<li>
			<input type="checkbox" name='<%#"chkPermission" + DataBinder.Eval(Container, "ItemIndex").ToString() %>' id='<%#"chkPermission" + DataBinder.Eval(Container, "ItemIndex").ToString() %>' value='<%#((AssignedPermission)Container.DataItem).Permission.Letter %>' <%#(((AssignedPermission)Container.DataItem).HasPermission ? "checked='true'" : "") %> />
			<label for='<%#"chkPermission" + DataBinder.Eval(Container, "ItemIndex").ToString() %>' class='checkbox inline <%#(((AssignedPermission)Container.DataItem).HasPermission ? "activePermission" : "") %>'>
				<%# umbraco.ui.GetText(((AssignedPermission)Container.DataItem).Permission.Alias) %>
			</label>
		</li>
	</ItemTemplate>
	<FooterTemplate>
		</ul>
	</FooterTemplate>
</asp:Repeater>