<%@ Control Language="c#" AutoEventWireup="True" CodeBehind="member.ascx.cs" Inherits="umbraco.cms.presentation.create.controls.member"
    TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<asp:ValidationSummary runat="server" DisplayMode="BulletList" ID="validationSummary"
    CssClass="error"></asp:ValidationSummary>
<asp:Literal ID="nameLiteral" runat="server"></asp:Literal>:<asp:RequiredFieldValidator
    ID="nameRequired" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator>
    <br />
<asp:TextBox ID="rename" runat="server" Width="350px" CssClass="bigInput"></asp:TextBox>
<asp:Panel ID="memberChooser" runat="server">
    <%=umbraco.ui.Text("choose")%>
    <%=umbraco.ui.Text("membertype")%>:<asp:RequiredFieldValidator ID="memberTypeRequired" ErrorMessage="*" ControlToValidate="nodeType"
        runat="server">*</asp:RequiredFieldValidator><br />
    <asp:ListBox ID="nodeType" runat="server" Width="350px" CssClass="bigInput" Rows="1"
        SelectionMode="Single"></asp:ListBox>
</asp:Panel>
<p>
    Login Name:<asp:RequiredFieldValidator ID="loginRequired" ErrorMessage="*" ControlToValidate="Login"
        runat="server">*</asp:RequiredFieldValidator>
        <asp:CustomValidator ID="loginExistsCheck" runat="server" ErrorMessage="*" ControlToValidate="Login" ValidateEmptyText="false" OnServerValidate="LoginExistsCheck"></asp:CustomValidator>
        <br />
    <asp:TextBox ID="Login" runat="server" Width="350px" CssClass="bigInput"></asp:TextBox><br />
</p>
<p>
    E-mail:    
    <asp:RequiredFieldValidator ID="emailRequired" ErrorMessage="*" ControlToValidate="Email"
        runat="server">*</asp:RequiredFieldValidator>
    <asp:CustomValidator ID="emailExistsCheck" runat="server" ErrorMessage="*" ControlToValidate="Email" ValidateEmptyText="false" OnServerValidate="EmailExistsCheck"></asp:CustomValidator>
    <asp:CustomValidator runat="server" ID="EmailValidator" OnServerValidate="EmailValidator_OnServerValidate"
        ControlToValidate="Email" 
        ErrorMessage="Invalid email address"
        Display="None"/>
    <br />
    <asp:TextBox ID="Email" runat="server" Width="350px" CssClass="bigInput"></asp:TextBox><br />
</p>
<p>
    Password: <em>
        <asp:Literal runat="server" ID="PasswordRules"></asp:Literal></em><asp:RequiredFieldValidator
            ID="passwordRequired" ControlToValidate="Password" runat="server">*</asp:RequiredFieldValidator><br />
    <asp:TextBox ID="Password" runat="server" Width="350px" CssClass="bigInput"></asp:TextBox><br />
</p>
<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" Style="visibility: hidden; display: none;" ID="Textbox1" />
<div style="padding-top: 15px;">
    <asp:Button ID="sbmt" runat="server" Style="margin-top: 14px" Width="90" OnClick="sbmt_Click">
    </asp:Button>
    &nbsp; <em>
        <%= umbraco.ui.Text("or") %></em> &nbsp;<a href="#" style="color: blue" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
</div>
