<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="script.ascx.cs" Inherits="umbraco.presentation.umbraco.create.script" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<input type="hidden" name="nodeType" value="-1"/>
<cc1:Pane runat="server">
    <cc1:PropertyPanel runat="server" Text="Name">
        <asp:TextBox ID="rename" runat="server" CssClass="bigInput input-large-type input-block-level"></asp:TextBox>
        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator>
    </cc1:PropertyPanel>

    <cc1:PropertyPanel runat="server" Text="Type">
        <asp:ListBox ID="scriptType" runat="server" CssClass="bigInput input-large-type input-block-level" Rows="1" SelectionMode="Single" />
    </cc1:PropertyPanel>
</cc1:Pane>

<cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
    <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
    <asp:Button ID="sbmt" CssClass="btn btn-primary" runat="server" Text='<%#umbraco.ui.Text("create") %>'></asp:Button>
</cc1:Pane>