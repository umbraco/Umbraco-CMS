<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="passwordChanger.ascx.cs" Inherits="umbraco.controls.passwordChanger" %>

<script type="text/javascript">
    (function ($) {
        Umbraco.Sys.registerNamespace("Umbraco.Controls");
        Umbraco.Controls.PasswordChanger = {
            toggle: function (e) {
                if (!$("#umbPasswordChanger").is(":visible")) {
                    ValidatorEnable(document.getElementById('<%=CompareValidator1.ClientID %>'), true);
                    $(e).closest(".propertyItem").replaceWith($("#umbPasswordChanger"));
                    $("#umbPasswordChanger").show();
                    $(e).hide();
                }                
            }
        };
    })(jQuery);
</script>

<a href="#" onclick="Umbraco.Controls.PasswordChanger.toggle(this);">Change password</a><br />

<div class="propertyItem" id="umbPasswordChanger" style="display: none;">

    <div class="propertyItemheader"><%=umbraco.ui.GetText("user", "newPassword")%></div>
    <div class="propertyItemContent">
        <asp:TextBox ID="umbPasswordChanger_passwordNew" autocomplete="off" AutoCompleteType="None" TextMode="password" runat="server"></asp:TextBox>
    </div>

    <div class="propertyItemheader"><%=umbraco.ui.GetText("user", "confirmNewPassword")%></div>
    <div class="propertyItemContent">
        <asp:TextBox ID="umbPasswordChanger_passwordNewConfirm" autocomplete="off" AutoCompleteType="None" TextMode="password" runat="server"></asp:TextBox>
        <asp:CompareValidator ID="CompareValidator1" runat="server" Enabled="False" ErrorMessage="Passwords must match" ControlToValidate="umbPasswordChanger_passwordNew"
            ControlToCompare="umbPasswordChanger_passwordNewConfirm" Operator="Equal"></asp:CompareValidator>
    </div>
</div>
