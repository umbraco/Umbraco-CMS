<%@ Control Language="c#" AutoEventWireup="True" Codebehind="DefaultUser.ascx.cs" Inherits="Umbraco.Web.UI.Install.Steps.DefaultUser" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<asp:Placeholder ID="identify" Runat="server" Visible="True">

<!-- create box -->
					<div class="tab main-tabinfo">

						<div class="container">

							<h1>Create User</h1>
							<div class="create-hold">
								<p>You can now setup a new admin user to log into Umbraco, we recommend using a stong password for this (a password which is more than 4 characters and contains a mix of letters, numbers and symbols).
                                Please make a note of the chosen password.</p>
								<p>The password can be changed once you have completed the installation and logged into the admin interface.</p>
							</div>

						</div>

						<div class="database-hold">

							<form action="#">

								<fieldset>

									<div class="container">

										<div class="instruction-hold">

											<div class="row">
                                                <asp:label AssociatedControlID="tb_name" runat="server">Name:</asp:label>
												<span><asp:TextBox ID="tb_name" CssClass="text" type="text" Text="admin" runat="server"  /></span>
                                                <asp:RequiredFieldValidator Display="Dynamic" CssClass="invalidaing" ControlToValidate="tb_name" runat="server" ErrorMessage="Name is a mandatory field" />
											</div>

											<div class="row">
												<asp:label AssociatedControlID="tb_email" runat="server">Email:</asp:label>
												<span><asp:TextBox ID="tb_email" CssClass="text" type="text" Text="admin@example.com" runat="server"  /></span>
                                                <asp:RequiredFieldValidator Display="Dynamic" CssClass="invalidaing" ControlToValidate="tb_email" runat="server" ErrorMessage="Email is a mandatory field" />
											</div>

											<div class="row">
												<asp:label AssociatedControlID="tb_login" runat="server">Username:</asp:label>
												<span><asp:TextBox ID="tb_login" CssClass="text" type="text" Text="admin" runat="server"  /></span>
												<asp:RequiredFieldValidator Display="Dynamic" CssClass="invalidaing" ControlToValidate="tb_login" runat="server" ErrorMessage="Username is a mandatory field" />
											</div>

											<div class="row" style="height: 35px; overflow: hidden;">
	                                            <asp:label AssociatedControlID="tb_password" runat="server">Password:</asp:label>
												<span><asp:TextBox ID="tb_password" CssClass="text" TextMode="Password" type="text" Text="" runat="server"  /></span>
												<asp:RequiredFieldValidator Display="Dynamic" CssClass="invalidaing" ControlToValidate="tb_password" runat="server" ErrorMessage="Password is a mandatory field" />
                                                <asp:CustomValidator ID="PasswordValidator" Display="Dynamic" CssClass="invalidaing" runat="server" />
											</div>

											<div class="row">
												<asp:Label AssociatedControlID="tb_password_confirm" runat="server">Confirm Password:</asp:label>
												<span><asp:TextBox ID="tb_password_confirm" CssClass="text" TextMode="Password" type="text" Text="" runat="server"  /></span>
                                                <asp:RequiredFieldValidator Display="Dynamic" CssClass="invalidaing" ControlToValidate="tb_password_confirm" runat="server" ErrorMessage="Confirm Password is a mandatory field" />
                                                <asp:CompareValidator Display="Dynamic" CssClass="invalidaing" ControlToCompare="tb_password" ControlToValidate="tb_password_confirm" ErrorMessage="The passwords must be identical" runat="server" />
											</div>

											<div class="check-hold">
                                            	<asp:CheckBox ID="cb_newsletter" runat="server" Checked="true"  /> 
                                                <asp:label AssociatedControlID="cb_newsletter" runat="server">Sign up for our monthly newsletter</asp:label>
											</div>
										</div>
									</div>
									<footer class="btn-box">
										<div class="t">&nbsp;</div>
                                        <asp:LinkButton CssClass="btn-create" runat="server" onclick="ChangePasswordClick"><span>Create user</span></asp:linkbutton>
									</footer>
								</fieldset>
							</form>
						</div>
					</div>
</asp:Placeholder>

<script type="text/javascript">
  $(document).ready(function() {
                 jQuery("#<%= tb_password.ClientID %>").passStrength({
                     shortPass:     "invalidaing",
                     badPass:       "invalidaing",
                     goodPass:      "validaing",
                     strongPass:    "validaing",
                     baseStyle: "basevalidaing",
                     minLength: <%=CurrentProvider.MinRequiredPasswordLength %>,
					userid:         jQuery("#<%= tb_login.ClientID %>").val(),
					messageloc:		1
				});
			});
</script>
