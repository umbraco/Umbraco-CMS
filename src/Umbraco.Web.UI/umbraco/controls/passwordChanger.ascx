<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="passwordChanger.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Controls.PasswordChanger" %>

<script type="text/javascript">
    (function ($) {
        Umbraco.Sys.registerNamespace("Umbraco.Controls");
        var enablePassRetrieval = <%=Provider.EnablePasswordRetrieval.ToString().ToLower()%>;
        Umbraco.Controls.PasswordChanger = {
            toggle: function (e) {
                if (!$("#umbPasswordChanger").is(":visible")) {
                    this.togglePasswordInputValidators(true);
                    $(e).closest(".propertyItem").replaceWith($("#umbPasswordChanger"));
                    $("#umbPasswordChanger").show();
                    $("#<%=IsChangingPasswordField.ClientID%>").val("true");
                    $(e).hide();
                }                
            },
            togglePasswordInputValidators: function(enable) {
                if (enable) {
                    ValidatorEnable(document.getElementById('<%=NewPasswordRequiredValidator.ClientID %>'), true);
                    ValidatorEnable(document.getElementById('<%=ConfirmPasswordValidator.ClientID %>'), true);
                    ValidatorEnable(document.getElementById('<%=NewPasswordLengthValidator.ClientID %>'), true);
                    if (!enablePassRetrieval) {
                        var currPassVal = document.getElementById('<%=CurrentPasswordValidator.ClientID %>');
                        if (currPassVal) {
                            ValidatorEnable(currPassVal, true);   
                        }                        
                    }
                }
                else {
                    var currPassVal = document.getElementById('<%=CurrentPasswordValidator.ClientID %>');
                    if (currPassVal) {
                        ValidatorEnable(currPassVal, false);   
                    }                        
                    ValidatorEnable(document.getElementById('<%=ConfirmPasswordValidator.ClientID %>'), false);
                    ValidatorEnable(document.getElementById('<%=NewPasswordRequiredValidator.ClientID %>'), false);
                    ValidatorEnable(document.getElementById('<%=NewPasswordLengthValidator.ClientID %>'), false);                    
                }
            },
            toggleReset: function(isChecked) {
                if (isChecked) {
                    $("#passwordInputArea").hide();
                    this.togglePasswordInputValidators(false);
                }
                else {
                    $("#passwordInputArea").show();
                    this.togglePasswordInputValidators(true);
                }
            }
        };

        $(document).ready(function () {
            $("#changePasswordButton").click(function() {
                Umbraco.Controls.PasswordChanger.toggle(this);
            });
            $("#<%=ResetPasswordCheckBox.ClientID%>").change(function () {
                Umbraco.Controls.PasswordChanger.toggleReset(
                    $("#<%=ResetPasswordCheckBox.ClientID%>").is(":checked"));
            });
        });
        
    })(jQuery);
</script>

<a href="#" id="changePasswordButton">Change password</a><br />

<div class="propertyItem" id="umbPasswordChanger" style="display: none;">
    
    <asp:HiddenField runat="server" ID="IsChangingPasswordField" Value="false" />

    <asp:PlaceHolder runat="server" ID="ResetPlaceHolder" Visible="<%#Provider.EnablePasswordReset %>">
        <div class="propertyItemheader"><%=umbraco.ui.GetText("user", "resetPassword")%></div>
        <div class="propertyItemContent">
            <asp:CheckBox runat="server" ID="ResetPasswordCheckBox" />
        </div>
    </asp:PlaceHolder>

    <div id="passwordInputArea">
        <asp:PlaceHolder runat="server" ID="CurrentPasswordPlaceHolder" Visible="<%#ShowOldPassword %>">
            <div class="propertyItemheader"><%=umbraco.ui.GetText("user", "passwordCurrent")%></div>
            <div class="propertyItemContent">
                <asp:TextBox ID="umbPasswordChanger_passwordCurrent" autocomplete="off" AutoCompleteType="None" TextMode="password" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="CurrentPasswordValidator" Enabled="False" runat="server"
                    Display="Dynamic"
                    ControlToValidate="umbPasswordChanger_passwordCurrent"
                    ErrorMessage="Required" />
            </div>
        </asp:PlaceHolder>

        <div class="propertyItemheader"><%=umbraco.ui.GetText("user", "newPassword")%></div>
        <div class="propertyItemContent">
            <asp:TextBox ID="umbPasswordChanger_passwordNew" autocomplete="off" AutoCompleteType="None" TextMode="password" runat="server"></asp:TextBox>
            <asp:RequiredFieldValidator ID="NewPasswordRequiredValidator" Enabled="False" runat="server"
                Display="Dynamic"
                ControlToValidate="umbPasswordChanger_passwordNew"
                ErrorMessage="Required" />
            <asp:RegularExpressionValidator ID="NewPasswordLengthValidator" runat="server"
                Display="Dynamic"
                ControlToValidate="umbPasswordChanger_passwordNew"
                ErrorMessage='<%# "Minimum " + Provider.MinRequiredPasswordLength + " characters" %>'
                ValidationExpression='<%# ".{" + Provider.MinRequiredPasswordLength + "}.*" %>' />
        </div>

        <div class="propertyItemheader"><%=umbraco.ui.GetText("user", "confirmNewPassword")%></div>
        <div class="propertyItemContent">
            <asp:TextBox ID="umbPasswordChanger_passwordNewConfirm" autocomplete="off" AutoCompleteType="None" TextMode="password" runat="server"></asp:TextBox>
            <asp:CompareValidator ID="ConfirmPasswordValidator" runat="server" Enabled="False"
                Display="Dynamic"
                ErrorMessage="Passwords must match"
                ControlToValidate="umbPasswordChanger_passwordNew"
                ControlToCompare="umbPasswordChanger_passwordNewConfirm"
                Operator="Equal" />
        </div>
    </div>

</div>

<div id="Div1" runat="server" class="success" style="margin-top:10px;width:300px;" Visible="<%# string.IsNullOrWhiteSpace(ChangingPasswordModel.GeneratedPassword) == false %>">
    <p style="text-align: center">
        Password has been reset to<br/><br/> 
        <strong><%# ChangingPasswordModel.GeneratedPassword %></strong>
    </p>
</div>