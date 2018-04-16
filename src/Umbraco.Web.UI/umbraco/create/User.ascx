<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="User.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Create.User" %>
<%@ Import Namespace="umbraco" %>
<asp:ValidationSummary runat="server" DisplayMode="BulletList" ID="validationSummary" CssClass="error"></asp:ValidationSummary>
<p>
    Login Name:
    <asp:RequiredFieldValidator ID="loginRequired"
        ErrorMessage='<%#Services.TextService.Localize("errorHandling/errorMandatoryWithoutTab", new[] { "Login Name"}) %>'
        ControlToValidate="Login" runat="server">*</asp:RequiredFieldValidator>
    <asp:CustomValidator ID="loginExistsCheck" runat="server"
        ErrorMessage='<%#Services.TextService.Localize("errorHandling/errorExistsWithoutTab", new[] { "Login Name"}) %>'
        ControlToValidate="Login" ValidateEmptyText="false" OnServerValidate="LoginExistsCheck"></asp:CustomValidator>
    <br />
    <asp:TextBox ID="Login" runat="server" Width="350px" CssClass="bigInput"></asp:TextBox>
</p>
<p>
    E-mail:
    <asp:RequiredFieldValidator ID="emailRequired"
        ErrorMessage='<%#Services.TextService.Localize("errorHandling/errorMandatoryWithoutTab", new[] { "E-mail"}) %>'
        ControlToValidate="Email" runat="server">*</asp:RequiredFieldValidator>
    <asp:CustomValidator ID="emailExistsCheck" runat="server"
        ErrorMessage='<%#Services.TextService.Localize("errorHandling/errorExistsWithoutTab", new[] { "E-mail"}) %>'
        ControlToValidate="Email" ValidateEmptyText="false" OnServerValidate="EmailExistsCheck"></asp:CustomValidator>
    <asp:CustomValidator runat="server" ID="EmailValidator" OnServerValidate="EmailValidator_OnServerValidate"
        ControlToValidate="Email"
        ErrorMessage='<%#ui.Text("errorHandling", "errorRegExpWithoutTab", "E-mail", CurrentUser) %>'
        Display="None" />
    <br />
    <asp:TextBox ID="Email" runat="server" Width="350px" CssClass="bigInput"></asp:TextBox>
</p>
<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" Style="visibility: hidden; display: none;" ID="Textbox1" />
<div style="padding-top: 15px;">
    <asp:Button ID="sbmt" runat="server" Style="margin-top: 14px" Width="90" OnClick="sbmt_Click" Text='<%# Services.TextService.Localize("create") %>'></asp:Button>
    &nbsp; <em><%= Services.TextService.Localize("or") %></em> &nbsp;<a href="#" style="color: blue" onclick="UmbClientMgr.closeModalWindow()"><%=Services.TextService.Localize("cancel")%></a>
</div>
