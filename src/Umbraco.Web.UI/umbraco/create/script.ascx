<%@ Control Language="C#" AutoEventWireup="true" Inherits="umbraco.presentation.umbraco.create.script" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<input type="hidden" name="nodeType" value="-1"/>
<cc1:Pane runat="server">
    <cc1:PropertyPanel runat="server" Text="Name (use / to make folders)">
        <asp:TextBox ID="rename" runat="server" CssClass="bigInput input-large-type input-block-level"></asp:TextBox>        
        
        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" 
            CssClass="text-error" Display="Dynamic"
            ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator>
        <asp:RegularExpressionValidator runat="server" ID="EndsWithValidator" 
            CssClass="text-error" Display="Dynamic"
            ErrorMessage="Cannot end with '/'" ControlToValidate="rename" ValidationExpression=".*[^/]$">
            Cannot end with '/'
        </asp:RegularExpressionValidator>
        <asp:RegularExpressionValidator runat="server" ID="ContainsValidator" 
            EnableViewState="false"
            CssClass="text-error" Display="Dynamic"
            ErrorMessage="Cannot end with '/'" ControlToValidate="rename" ValidationExpression="^((?!\.).)*$">
            Cannot contain a '.'
        </asp:RegularExpressionValidator>

    </cc1:PropertyPanel>

    <cc1:PropertyPanel runat="server" Text="Type">
        <asp:ListBox ID="scriptType" runat="server" CssClass="bigInput input-large-type input-block-level" Rows="1" SelectionMode="Single" />
    </cc1:PropertyPanel>
</cc1:Pane>

<cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
    <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
    <asp:Button ID="sbmt" CssClass="btn btn-primary" runat="server" Text='<%#umbraco.ui.Text("create") %>' OnClick="SubmitClick"></asp:Button>
</cc1:Pane>