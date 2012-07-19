<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.ascx.cs" Inherits="umbraco.presentation.umbraco.dashboard.ChangePassword" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<h3><%=umbraco.ui.Text("changePassword") %></h3>
<asp:Panel ID="changeForm" Runat="server" Visible="true">
<umb:JsInclude ID="JsInclude2" runat="server" FilePath="passwordStrength/passwordstrength.js" PathNameAlias="UmbracoClient" Priority="11" />
<p><%=umbraco.ui.Text("changePasswordDescription") %></p>
<asp:Panel ID="errorPane" runat="server" Visible="false">
<div class="error"><p><asp:Literal ID="errorMessage" runat="server"></asp:Literal></p></div>
</asp:Panel>
<ol class="form">

  <li style="height: 20px;">
  <asp:Label runat="server" AssociatedControlID="password" ID="Label1"><%=umbraco.ui.Text("username") %>:</asp:Label> <strong id="username"><%=umbraco.BusinessLogic.User.GetCurrent().Name%></strong>
  </li>
   <li>
    <asp:Label runat="server" AssociatedControlID="password" ID="passwordLabel"><%=umbraco.ui.Text("passwordEnterNew") %>:</asp:Label>
    <asp:TextBox id="password" TextMode="password" CssClass="textfield" Runat="server"></asp:TextBox>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="password" ID="passwordvalidator" ValidationGroup="changepass">*</asp:RequiredFieldValidator>
    </li>
   <li>
    <asp:Label runat="server" AssociatedControlID="confirmpassword" ID="confirmpasswordlabel"><%=umbraco.ui.Text("passwordConfirm") %>:</asp:Label>
    <asp:TextBox id="confirmpassword" TextMode="password" CssClass="textfield" Runat="server"></asp:TextBox>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="confirmpassword" ID="confirmpasswordvalidator" ValidationGroup="changepass">*</asp:RequiredFieldValidator>
    <asp:CompareValidator ID="CompareValidator1" runat="server" ControlToValidate="confirmpassword" ControlToCompare="password" ValidationGroup="changepass"><%=umbraco.ui.Text("passwordMismatch") %></asp:CompareValidator>
    </li>
</ol>
        
<p>
<asp:Button id="changePassword" Runat="server" Text="Change Password" OnClientClick="showProgress(this,'loadingBar'); return true;" onclick="changePassword_Click" ValidationGroup="changepass"></asp:Button>
</p>
<script type="text/javascript">
    jQuery(document).ready(function () {

        //ADVANCE
        jQuery("#<%= password.ClientID %>").passStrength({
            shortPass: "error",
            badPass: "error",
            goodPass: "success",
            strongPass: "success",
            baseStyle: "passtestresult",
            userid: "<%=umbraco.BusinessLogic.User.GetCurrent().Name%>",
            messageloc: 1
        });
    });
		</script>
</asp:Panel>

<asp:Panel ID="passwordChanged" Runat="server" Visible="False">
<p><br /></p>
<div class="success"><p><%=umbraco.ui.Text("passwordChanged") %>!</p></div>
</asp:Panel>

