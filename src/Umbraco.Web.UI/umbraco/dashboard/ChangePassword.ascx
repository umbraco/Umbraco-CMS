﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.ascx.cs" Inherits="umbraco.presentation.umbraco.dashboard.ChangePassword" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<umb:CssInclude runat="server" FilePath="propertypane/style.css" PathNameAlias="UmbracoClient" />
<umb:JsInclude ID="JsInclude2" runat="server" FilePath="passwordStrength/passwordstrength.js" PathNameAlias="UmbracoClient" Priority="11" />

<script type="text/javascript">
	jQuery(document).ready(function () {
		jQuery("#<%= password.ClientID %>").passStrength({
			shortPass: "error",
			badPass: "error",
			goodPass: "success",
			strongPass: "success",
			baseStyle: "passtestresult",
			minLength: <%=Provider.MinRequiredPasswordLength %>,
			userid: "<%=umbraco.BusinessLogic.User.GetCurrent().Name%>",
			messageloc: 1
		});
	});
</script>

<div class="dashboardWrapper">
	<h2><%=umbraco.ui.Text("changePassword") %></h2>
	<img src="./dashboard/images/membersearch.png" alt="Users" class="dashboardIcon" />
	<asp:Panel ID="changeForm" Runat="server" Visible="true">
		<p><%=umbraco.ui.Text("changePasswordDescription") %></p>
		<asp:Panel ID="errorPane" runat="server" Visible="false">
			<div class="error">
				<p><asp:Literal ID="errorMessage" runat="server"/></p>
			</div>
		</asp:Panel>
		<ol class="form">
			<li style="height: 20px;">
				<span>
					<asp:Label runat="server" ID="Label1"><%=umbraco.ui.Text("username") %>:</asp:Label>
					<strong id="username"><%=umbraco.BusinessLogic.User.GetCurrent().Name%></strong>
				</span>
			</li>
			<li>
				<span>
					<asp:Label runat="server" AssociatedControlID="currentpassword" ID="Label2"><%=umbraco.ui.Text("passwordCurrent") %>:</asp:Label>
					<asp:TextBox id="currentpassword" TextMode="password" CssClass="textfield" Runat="server"></asp:TextBox>
					<asp:RequiredFieldValidator runat="server" ControlToValidate="currentpassword" ID="RequiredFieldValidator1" ValidationGroup="changepass">*</asp:RequiredFieldValidator>
				</span>
			</li>			
			<li>
				<span>
					<asp:Label runat="server" AssociatedControlID="password" ID="passwordLabel"><%=umbraco.ui.Text("passwordEnterNew") %>:</asp:Label>
					<asp:TextBox id="password" TextMode="password" CssClass="textfield" Runat="server"></asp:TextBox>
					<asp:RequiredFieldValidator runat="server" ControlToValidate="password" ID="passwordvalidator" ValidationGroup="changepass">*</asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="NewPasswordLengthValidator" runat="server"
                        ValidationGroup="changepass"
                        Display="None"
                        ControlToValidate="password"
                        ErrorMessage='<%# "Minimum " + Provider.MinRequiredPasswordLength + " characters" %>'
                        ValidationExpression='<%# ".{" + Provider.MinRequiredPasswordLength + "}.*" %>' />
				</span>
			</li>
			<li>
				<span>
					<asp:Label runat="server" AssociatedControlID="confirmpassword" ID="confirmpasswordlabel"><%=umbraco.ui.Text("passwordConfirm") %>:</asp:Label>
					<asp:TextBox id="confirmpassword" TextMode="password" CssClass="textfield" Runat="server"></asp:TextBox>
					<asp:CompareValidator ID="CompareValidator1" runat="server" ControlToValidate="confirmpassword" ControlToCompare="password" ValidationGroup="changepass" CssClass="error"><%=umbraco.ui.Text("passwordMismatch") %></asp:CompareValidator>
				</span>
			</li>
		</ol>
		<p>
			<asp:Button id="changePassword" Runat="server" Text="Change Password" onclick="changePassword_Click" ValidationGroup="changepass"></asp:Button>
		</p>
	</asp:Panel>
	<asp:Panel ID="passwordChanged" Runat="server" Visible="False">
		<div class="success"><p><%=umbraco.ui.Text("passwordChanged") %>!</p></div>
	</asp:Panel>
</div>
