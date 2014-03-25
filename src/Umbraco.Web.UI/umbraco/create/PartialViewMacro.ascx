<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PartialViewMacro.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Create.PartialViewMacro" %>
<%@ Import Namespace="umbraco" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<cc1:Pane runat="server">
    <cc1:PropertyPanel runat="server" Text="Filename (without .cshtml)">
        <asp:TextBox ID="FileName" runat="server" CssClass="bigInput input-large-type input-block-level"></asp:TextBox>
        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ErrorMessage="*" ControlToValidate="FileName" runat="server">*</asp:RequiredFieldValidator>
    </cc1:PropertyPanel>

    <cc1:PropertyPanel runat="server" Text="Choose a snippet:">
        <asp:ListBox ID="PartialViewTemplate" runat="server" CssClass="bigInput input-large-type input-block-level" Rows="1" SelectionMode="Single" />
    </cc1:PropertyPanel>

    <cc1:PropertyPanel runat="server">
        <asp:CheckBox ID="CreateMacroCheckBox" runat="server" Checked="true" Text="Create Macro"></asp:CheckBox>
        <asp:CustomValidator ErrorMessage="<br/>A macro already exists with the specified name" ID="MacroExistsValidator" 
            Display="Dynamic" ForeColor="red"
            runat="server" OnServerValidate="MacroExistsValidator_OnServerValidate"/>
    </cc1:PropertyPanel>
</cc1:Pane>

<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" Style="visibility: hidden; display: none;" ID="Textbox2" />
<input type="hidden" name="nodeType" value="-1">

<cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
    <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
    <asp:Button ID="SubmitButton" CssClass="btn btn-primary" runat="server" OnClick="SubmitButton_Click" Text='<%#ui.Text("create") %>'></asp:Button>
</cc1:Pane>
