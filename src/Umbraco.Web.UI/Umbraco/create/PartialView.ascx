<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PartialView.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Create.PartialView" %>
<%@ Import Namespace="umbraco" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<cc1:Pane runat="server">
    <cc1:PropertyPanel runat="server" Text="Filename (without .cshtml, use / to make folders)">
        <asp:TextBox ID="FileName" runat="server" CssClass="bigInput input-large-type input-block-level"></asp:TextBox>
        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" 
            CssClass="text-error" Display="Dynamic"
            ErrorMessage="*" ControlToValidate="FileName" runat="server">*</asp:RequiredFieldValidator>
        <asp:RegularExpressionValidator runat="server" ID="EndsWithValidator" 
            CssClass="text-error" Display="Dynamic"
            ErrorMessage="Cannot end with '/'" ControlToValidate="FileName" ValidationExpression=".*[^/\.]$">
            Cannot end with '/' or '.'
        </asp:RegularExpressionValidator>
    </cc1:PropertyPanel>

    <cc1:PropertyPanel runat="server" Text="Choose a snippet:">
        <asp:ListBox ID="PartialViewTemplate" runat="server" CssClass="bigInput input-large-type input-block-level" Rows="1" SelectionMode="Single"/>
    </cc1:PropertyPanel>
</cc1:Pane>

<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" Style="visibility: hidden; display: none;" ID="Textbox1" />
<input type="hidden" name="nodeType" value="-1">


<cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
    <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
    <asp:Button ID="sbmt" runat="server" CssClass="btn btn-primary" Text="Create" OnClick="SubmitButton_Click"></asp:Button>
</cc1:Pane>
