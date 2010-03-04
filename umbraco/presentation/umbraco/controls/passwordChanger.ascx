<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="passwordChanger.ascx.cs" Inherits="umbraco.controls.passwordChanger" %>

<a href="#" onclick="if (document.getElementById('umbPasswordChanger').style.display == '' || document.getElementById('umbPasswordChanger').style.display == 'none') {document.getElementById('umbPasswordChanger').style.display = 'block'; this.style.display = 'none';}">Change password</a><br />

<div id="umbPasswordChanger" style="display: none;">
<table>
    <tr><th style="width: 270px;">New Password:</th><td style="width: 359px">
    <asp:TextBox ID="umbPasswordChanger_passwordNew" autocomplete="off"  AutoCompleteType="None" TextMode="password" runat="server"></asp:TextBox>
    </td></tr>
    <tr><th>Confirm new Password:</th><td style="width: 359px">
    <asp:TextBox ID="umbPasswordChanger_passwordNewConfirm" autocomplete="off"  AutoCompleteType="None" TextMode="password" runat="server"></asp:TextBox>
    <asp:CompareValidator ID="CompareValidator1" runat="server" ErrorMessage="Passwords must match" ControlToValidate="umbPasswordChanger_passwordNew" 
    ControlToCompare="umbPasswordChanger_passwordNewConfirm" Operator="Equal"></asp:CompareValidator>
    </td></tr>
</table>
</div>