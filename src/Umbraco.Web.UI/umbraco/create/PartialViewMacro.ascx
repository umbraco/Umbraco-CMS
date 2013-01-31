<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PartialViewMacro.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Create.PartialViewMacro" %>
<%@ Import Namespace="umbraco" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<umb:CssInclude runat="server" FilePath="Dialogs/CreateDialog.css" PathNameAlias="UmbracoClient" />

Filename (without .cshtml):
<asp:RequiredFieldValidator ID="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="FileName" runat="server">*</asp:RequiredFieldValidator>

<%--<input type="hidden" name="nodeType" value="-1">--%>
<div>
    <asp:TextBox ID="FileName" runat="server" CssClass="bigInput"></asp:TextBox>
</div>
<%--<div>
    Choose a template:<br />
    <asp:ListBox ID="PartialViewTemplate" runat="server" Rows="1" SelectionMode="Single">
        <asp:ListItem Value="clean.xslt">Clean</asp:ListItem>
    </asp:ListBox>
</div>--%>

<div>
    <asp:CheckBox ID="CreateMacroCheckBox" runat="server" Checked="true" Text="Create Macro"></asp:CheckBox>
</div>

<div class="submit-footer">
    <asp:Button ID="SubmitButton" runat="server" OnClick="SubmitButton_Click" Text='<%#ui.Text("create") %>'></asp:Button>
    &nbsp; <em><%= umbraco.ui.Text("or") %></em> &nbsp;
    <a href="#" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
</div>