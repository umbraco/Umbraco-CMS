<%@ Control Language="c#" AutoEventWireup="True" Inherits="umbraco.cms.presentation.create.controls.language" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<input type="hidden" name="nodeType"/>

<cc1:Pane runat="server">
    <cc1:PropertyPanel runat="server" Text="Name" id="pp1">
        <asp:DropDownList ID="Cultures" runat="server" CssClass="input-block-level"></asp:DropDownList>
        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="Cultures" runat="server">*</asp:RequiredFieldValidator>
    </cc1:PropertyPanel>
</cc1:Pane>

<cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
    <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
    <asp:Button ID="sbmt" CssClass="btn btn-primary" OnClick="sbmt_Click" runat="server" Text='<%#umbraco.ui.Text("create") %>'></asp:Button>
</cc1:Pane>

    
