<%@ Control Language="c#" AutoEventWireup="True" Codebehind="defaultUser.ascx.cs" Inherits="umbraco.presentation.install.steps.defaultUser" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<h1>Step 4/5: Check umbraco security</h1>
<asp:Panel ID="identify" Runat="server" Visible="True">
<p>
  umbraco creates a default user with a login <strong>('admin')</strong> and password <strong>('default')</strong>. It's <strong>important</strong> that the password is 
  changed to something unique.
</p>

<p>
  This step will check the default user's password and suggest if it needs to be changed.
</p>

<asp:Literal id="identifyResult" Runat="server"></asp:Literal>
</asp:Panel>

<asp:Panel ID="changeForm" Runat="server" Visible="false">

<ol class="form">
  <li style="height: 20px;">
  <asp:Label runat="server" AssociatedControlID="password" ID="Label1">User name:</asp:Label> <strong id="username">admin</strong>
  </li>
   <li>
    <asp:Label runat="server" AssociatedControlID="password" ID="passwordLabel">Enter new password:</asp:Label>
    <asp:TextBox id="password" TextMode="password" CssClass="textfield" Runat="server"></asp:TextBox>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="password" ID="passwordvalidator">*</asp:RequiredFieldValidator>
    </li>
   <li>
    <asp:Label runat="server" AssociatedControlID="confirmpassword" ID="confirmpasswordlabel">Confirm password:</asp:Label>
    <asp:TextBox id="confirmpassword" TextMode="password" CssClass="textfield" Runat="server"></asp:TextBox>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="confirmpassword" ID="confirmpasswordvalidator">*</asp:RequiredFieldValidator>
    <asp:CompareValidator runat="server" ControlToValidate="confirmpassword" ControlToCompare="password">*</asp:CompareValidator>
    </li>
</ol>
        
<p>
<asp:Button id="changePassword" Runat="server" Text="Change Password" OnClientClick="showProgress(this,'loadingBar'); return true;" onclick="changePassword_Click"></asp:Button>
</p>

</asp:Panel>

<asp:Panel ID="passwordChanged" Runat="server" Visible="False">
<div class="success"><p>The password is changed!</p></div>
</asp:Panel>

<script type="text/javascript">
  $(document).ready(function() {
			
				//ADVANCE
				jQuery("#<%= password.ClientID %>").passStrength({
					shortPass: 		"error",
					badPass:		"error",
					goodPass:		"success",
					strongPass:		"success",
					baseStyle:		"passtestresult",
					userid:			"admin",
					messageloc:		1
				});
			});
		</script>
